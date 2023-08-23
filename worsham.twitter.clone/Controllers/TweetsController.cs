using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using worsham.twitter.clone.Models;

namespace worsham.twitter.clone.Controllers
{
    public class TweetsController : Controller
    {
        private readonly TwitterCloneContext _context;
        private readonly ILogger<LikesController> _logger;

        public TweetsController(TwitterCloneContext context, ILogger<LikesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Displays the user's tweets and the tweets from the people they follow on the tweets feed page.
        /// </summary>
        /// <returns>The tweets feed view containing the tweets.</returns>
        public async Task<IActionResult> Index()
        {
            try
            {
                // get all tweets where the tweeter is the current user and all tweets from the people they follow
                int? currentUserId = HttpContext.Session.GetInt32("UserId");

                // Get the IDs of the people the current user follows
                IQueryable<int>? followedUserIds = _context.Follows
                    .Where(f => f.FollowerUserId == currentUserId)
                    .Select(f => f.FollowedUserId);

                // Get tweets by the current user and the users they follow
                List<Tweets>? tweets = _context.Tweets
                    .Where(t => t.TweeterId == currentUserId || followedUserIds.Contains(t.TweeterId)).Include(t => t.Comments).Include(t => t.Likes).Include(t => t.ReTweets).OrderBy(t => t.CreationDateTime).ToList();

                _logger.LogInformation("Number of tweets retrieved: {TweetCount}", tweets.Count);
                _logger.LogInformation("Number of followed users: {FollowedUserCount}", followedUserIds.Count());

                List<TweetModel> tweetModels = new();

                // Iterate through the retrieved tweets and create corresponding TweetModel objects
                foreach (var tweet in tweets)
                {
                    tweetModels.Add(new TweetModel()
                    {
                        Id = tweet.Id,
                        TimeSincePosted = DateTime.Now - tweet.CreationDateTime,
                        Content = tweet.Content,
                        TweeterUserName = _context.Users.FirstOrDefault(u => u.Id == tweet.TweeterId)?.UserName,
                        Likes = tweet.Likes.ToList(),
                        Comments = tweet.Comments.ToList(),
                        Retweets = tweet.ReTweets.ToList()
                    });
                }

                TweetsFeedViewModel tweetsFeedViewModel = new TweetsFeedViewModel(
                    HasErrors: false,
                    ValidationErrors: Enumerable.Empty<string>(),
                    Tweets: tweetModels,
                    Post: new PostModel()
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

        // GET: Tweets/Create
        public IActionResult Create()
        {
            ViewData["TweeterId"] = new SelectList(_context.Users, "Id", "Email");
            return View();
        }

        // POST: Tweets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Content,CreationDateTime,TweeterId")] Tweets tweets)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tweets);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TweeterId"] = new SelectList(_context.Users, "Id", "Email", tweets.TweeterId);
            return View(tweets);
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
