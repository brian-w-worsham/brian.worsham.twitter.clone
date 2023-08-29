using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using worsham.twitter.clone.Models;
using worsham.twitter.clone.Models.EntityModels;

namespace worsham.twitter.clone.Controllers
{
    public class TweetsController : Controller
    {
        private readonly TwitterCloneContext _context;
        private readonly ILogger<LikesController> _logger;
        private int? _currentUserId;

        public TweetsController(TwitterCloneContext context, ILogger<LikesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _currentUserId = HttpContext.Session.GetInt32("UserId");
            base.OnActionExecuting(context);
        }

        /// <summary>
        /// Displays the user's tweets and the tweets from the people they follow on the tweets feed page.
        /// </summary>
        /// <returns>The tweets feed view containing the tweets.</returns>
        public async Task<IActionResult> Index()
        {
            try
            {
                // Get the IDs of the people the current user follows
                IQueryable<int>? followedUserIds = _context.Follows
                    .Where(f => f.FollowerUserId == _currentUserId)
                    .Select(f => f.FollowedUserId);

                // Get tweets by the current user and the users they follow
                List<Tweets>? tweets = await _context.Tweets.Where(t => t.TweeterId == _currentUserId || followedUserIds.Contains(t.TweeterId)).Include(t => t.Comments).Include(t => t.Likes).Include(t => t.ReTweets).OrderByDescending(t => t.CreationDateTime).ToListAsync();

                // Create a List of ReTweets from users the current user follows
                List<ReTweets>? reTweets = await _context.ReTweets.Where(rt => followedUserIds.Contains(rt.RetweeterId)).Include(rt => rt.OriginalTweet).Include(rt => rt.Retweeter).OrderByDescending(rt => rt.ReTweetCreationDateTime).ToListAsync();

                // Extract OriginalTweets from reTweets
                List<Tweets> reTweetedTweets = reTweets.Select(rt => rt.OriginalTweet).ToList();

                // Merge the sorted lists
                List<Tweets> combinedAndSortedTweets = new List<Tweets>();
                combinedAndSortedTweets.AddRange(reTweetedTweets);
                combinedAndSortedTweets.AddRange(tweets);

                // Sort the combined list by maximum of ReTweetCreationDateTime in descending order
                combinedAndSortedTweets.Sort((item1, item2) =>
                {
                    DateTime minCreationTime1 = item1.CreationDateTime;
                    if (item1.ReTweets.Any())
                    {
                        minCreationTime1 = item1.ReTweets.Min(rt => rt.ReTweetCreationDateTime);
                    }

                    DateTime minCreationTime2 = item2.CreationDateTime;
                    if (item2.ReTweets.Any())
                    {
                        minCreationTime2 = item2.ReTweets.Min(rt => rt.ReTweetCreationDateTime);
                    }

                    return minCreationTime2.CompareTo(minCreationTime1);
                });

                _logger.LogInformation("Combined and sorted {Count} tweets and retweets.", combinedAndSortedTweets.Count);
                _logger.LogInformation("Number of followed users: {FollowedUserCount}", followedUserIds.Count());

                List<TweetModel> tweetModels = new();

                // Iterate through the retrieved tweets and create corresponding TweetModel objects
                foreach (var tweet in combinedAndSortedTweets)
                {
                    _logger.LogInformation("Processing tweet with ID {TweetId}.", tweet.Id);
                    tweetModels.Add(new TweetModel()
                    {
                        Id = tweet.Id,
                        TimeSincePosted = DateTime.UtcNow - tweet.CreationDateTime,
                        Content = tweet.Content,
                        TweeterUserId = tweet.ReTweets.Any() ? tweet.ReTweets.First().OriginalTweet.TweeterId : tweet.TweeterId,
                        TweeterUserName = (await _context.Users.FirstOrDefaultAsync(u => u.Id == tweet.TweeterId))?.UserName,
                        Likes = tweet.Likes.ToList(),
                        Comments = tweet.Comments.ToList(),
                        Retweets = tweet.ReTweets.ToList()
                    });
                    // store the user's ProfilePictureUrl in the ViewData dictionary
                    ViewData[tweet.Id.ToString()] = (await _context.Users.FirstOrDefaultAsync(u => u.Id == tweet.TweeterId))?.ProfilePictureUrl ?? "\\default\\1.jpg";
                }

                tweetModels.Sort((item1, item2) => item1.TimeSincePosted.CompareTo(item2.TimeSincePosted));

                TweetsFeedViewModel tweetsFeedViewModel = new TweetsFeedViewModel(
                    HasErrors: false,
                    ValidationErrors: Enumerable.Empty<string>(),
                    Tweets: tweetModels,
                    Post: new PostModel(),
                    currentUserId: _currentUserId
                );

                return View(tweetsFeedViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tweets in the Index method");
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Tweets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Tweets == null)
            {
                return NotFound();
            }

            var tweets = await _context.Tweets
                .Include(t => t.Tweeter)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tweets == null)
            {
                return NotFound();
            }

            return View(tweets);
        }

        /// <summary>
        /// Handles the HTTP POST request to create a new tweet.
        /// </summary>
        /// <param name="postModel">The data model containing the content of the new tweet.</param>
        /// <returns>
        /// If the ModelState is valid, adds a new tweet to the database and redirects to the tweets feed page.
        /// If the ModelState is invalid, logs the validation errors, and redirects to the tweets feed page.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Content")] PostModel postModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(new Tweets()
                    {
                        Content = postModel.Content,
                        CreationDateTime = DateTime.UtcNow,
                        TweeterId = (int)_currentUserId
                    });
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // If ModelState is invalid, log validation errors
                    foreach (var modelState in ModelState.Values)
                    {
                        foreach (var error in modelState.Errors)
                        {
                            _logger.LogError("Model Error: {ErrorMessage}", error.ErrorMessage);
                        }
                    }
                    // Todo: render a notification to the user that the tweet creation failed
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                // Log any unexpected exceptions that might occur during tweet creation
                _logger.LogError(ex, "An error occurred while creating a new tweet.");

                //Todo: render a notification to the user that the tweet creation failed
                ViewData["TweetFailed"] = true;
                return RedirectToAction("Index", "Tweets");
            }
        }

        // GET: Tweets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tweets == null)
            {
                return NotFound();
            }

            var tweets = await _context.Tweets.FindAsync(id);
            if (tweets == null)
            {
                return NotFound();
            }
            ViewData["TweeterId"] = new SelectList(_context.Users, "Id", "Email", tweets.TweeterId);
            return View(tweets);
        }

        // POST: Tweets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Content,CreationDateTime,TweeterId")] Tweets tweets)
        {
            if (id != tweets.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tweets);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TweetsExists(tweets.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["TweeterId"] = new SelectList(_context.Users, "Id", "Email", tweets.TweeterId);
            return View(tweets);
        }

        // GET: Tweets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Tweets == null)
            {
                return NotFound();
            }

            var tweets = await _context.Tweets
                .Include(t => t.Tweeter)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tweets == null)
            {
                return NotFound();
            }

            return View(tweets);
        }

        // POST: Tweets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Tweets == null)
            {
                return Problem("Entity set 'TwitterCloneContext.Tweets'  is null.");
            }
            var tweets = await _context.Tweets.FindAsync(id);
            if (tweets != null)
            {
                _context.Tweets.Remove(tweets);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TweetsExists(int id)
        {
            return (_context.Tweets?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
