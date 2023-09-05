using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using worsham.twitter.clone.Models;
using worsham.twitter.clone.Models.EntityModels;
using worsham.twitter.clone.Services;
using static System.Net.Mime.MediaTypeNames;

namespace worsham.twitter.clone.Controllers
{
    public class TweetsController : TwitterController
    {
        private readonly TwitterCloneContext _context;
        private int? _currentUserId;

        public TweetsController(TwitterCloneContext context, ILogger<TweetsController> logger, IAuthorizationService authorizationService) : base(logger, authorizationService)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _currentUserId = HttpContext.Session.GetInt32("UserId");
            base.OnActionExecuting(context);
        }

        /// <summary>
        /// Retrieves and displays the user's feed of tweets and retweets on the Index page.
        /// </summary>
        /// <returns>
        /// An IActionResult representing the Index page with the user's tweet feed.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                if (_currentUserId == null)
                {
                    _logger.LogInformation("User is not logged in. Redirecting to Home/Index.");
                    return RedirectToAction("Index", "Home");
                }

                var followedUserIds = GetFollowedUserIds();

                var tweets = await GetTweetsAsync(followedUserIds);
                var reTweets = await GetReTweetsAsync(followedUserIds);

                var combinedAndSortedTweets = CombineAndSortTweets(tweets, reTweets);

                _logger.LogInformation("Combined and sorted {Count} tweets and retweets.", combinedAndSortedTweets.Count);
                _logger.LogInformation("Number of followed users: {FollowedUserCount}", followedUserIds.Count());

                var tweetModels = await CreateTweetModels(combinedAndSortedTweets);

                tweetModels.Sort((item1, item2) => item1.TimeSincePosted.CompareTo(item2.TimeSincePosted));

                var tweetsFeedViewModel = new TweetsFeedViewModel(
                    HasErrors: false,
                    ValidationErrors: Enumerable.Empty<string>(),
                    Tweets: tweetModels,
                    Post: new PostModel(),
                    currentUserId: _currentUserId
                );

                ViewData["page"] = "tweets";
                ViewData["errorNotification"] = TempData["errorNotification"]?.ToString() ?? "";

                return View(tweetsFeedViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tweets in the Index method");
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Retrieves the IDs of users followed by the current user.
        /// </summary>
        /// <returns>
        /// An IQueryable of integers representing the followed user IDs.
        /// </returns>
        private IQueryable<int> GetFollowedUserIds()
        {
            // Retrieve the IDs of users that the current user is following.
            var followedUserIds = _context.Follows
                .Where(f => f.FollowerUserId == _currentUserId)
                .Select(f => f.FollowedUserId);

            return followedUserIds;
        }

        /// <summary>
        /// Retrieves a list of tweets authored by the current user and the users they follow asynchronously.
        /// </summary>
        /// <param name="followedUserIds">An IQueryable of integers representing the followed user IDs.</param>
        /// <returns>
        /// An asynchronous task that returns a list of tweets that match the specified criteria.
        /// </returns>
        private async Task<List<Tweets>> GetTweetsAsync(IQueryable<int> followedUserIds)
        {
            return await _context.Tweets
                .Where(t => t.TweeterId == _currentUserId || followedUserIds.Contains(t.TweeterId))
                .Include(t => t.Comments)
                .Include(t => t.Likes)
                .Include(t => t.ReTweets)
                .OrderByDescending(t => t.CreationDateTime)
                .ToListAsync();
        }

        /// <summary>
        /// Asynchronously retrieves a list of retweets, including the original tweet, comments, likes, and retweets, for the current user and followed users.
        /// </summary>
        /// <param name="followedUserIds">A queryable collection of user IDs representing the users the current user is following.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result is a list of <see cref="ReTweets"/> objects containing the retweets and associated data.
        /// </returns>
        /// <remarks>
        /// This method fetches retweets from the database for the current user and the users they are following. It includes related data such as the original tweet,
        /// comments, likes, and retweets on the original tweet. The result is ordered by the creation date and time of the retweets in descending order.
        /// </remarks>
        private async Task<List<ReTweets>> GetReTweetsAsync(IQueryable<int> followedUserIds)
        {
            return await _context.ReTweets
                .Where(rt => rt.RetweeterId == _currentUserId || followedUserIds.Contains(rt.RetweeterId))
                .Include(rt => rt.OriginalTweet)
                .Include(rt => rt.OriginalTweet.Comments)
                .Include(rt => rt.OriginalTweet.Likes)
                .Include(rt => rt.OriginalTweet.ReTweets)
                .OrderByDescending(rt => rt.ReTweetCreationDateTime)
                .ToListAsync();
        }

        /// <summary>
        /// Combines and sorts a list of tweets and retweets chronologically.
        /// </summary>
        /// <param name="tweets">A list of tweets to be combined and sorted.</param>
        /// <param name="reTweets">A list of retweets to be combined and sorted.</param>
        /// <returns>
        /// A list of tweets and retweets combined and sorted by chronological order.
        /// </returns>
        private List<Tweets> CombineAndSortTweets(List<Tweets> tweets, List<ReTweets> reTweets)
        {
            var reTweetedTweets = reTweets.Select(rt => rt.OriginalTweet).ToList();
            var combinedAndSortedTweets = new List<Tweets>();
            combinedAndSortedTweets.AddRange(reTweetedTweets);
            combinedAndSortedTweets.AddRange(tweets);

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
            combinedAndSortedTweets = combinedAndSortedTweets.Distinct().ToList();

            return combinedAndSortedTweets;
        }

        /// <summary>
        /// Creates a list of TweetModel objects from a list of combined and sorted tweets.
        /// </summary>
        /// <param name="combinedAndSortedTweets">A list of combined and sorted tweets to be converted into TweetModel objects.</param>
        /// <returns>
        /// A list of TweetModel objects representing the tweets with additional information.
        /// </returns>
        private async Task<List<TweetModel>> CreateTweetModels(List<Tweets> combinedAndSortedTweets)
        {
            var tweetModels = new List<TweetModel>();
            foreach (var tweet in combinedAndSortedTweets)
            {
                _logger.LogInformation("Processing tweet with ID {TweetId}.", tweet.Id);
                var tweeter = await _context.Users.FirstOrDefaultAsync(u => u.Id == tweet.TweeterId);
                tweetModels.Add(new TweetModel()
                {
                    Id = tweet.Id,
                    TimeSincePosted = DateTime.UtcNow - tweet.CreationDateTime,
                    Content = tweet.Content,
                    TweeterUserId = tweet.ReTweets.Any() ? tweet.ReTweets.First().OriginalTweet.TweeterId : tweet.TweeterId,
                    TweeterUserName = tweeter?.UserName,
                    Likes = tweet.Likes.ToList(),
                    Comments = tweet.Comments.ToList(),
                    Retweets = tweet.ReTweets.ToList()
                });
                ViewData[tweet.Id.ToString()] = tweeter?.ProfilePictureUrl ?? "\\default\\1.jpg";
            }
            return tweetModels;
        }


        /// <summary>
        /// Displays details of a tweet with the specified ID, if authorized as an admin.
        /// </summary>
        /// <param name="id">The ID of the tweet to display details for.</param>
        /// <returns>
        /// If the user is authorized as an admin, displays the details of the tweet with the specified ID.
        /// If the user is not authorized as an admin, redirects the user to the appropriate page using the <see cref="RedirectIfNotAdmin"/> method.
        /// </returns>
        /// <remarks>
        /// This method displays the details of a tweet with the specified ID, provided that the user is authorized as an admin.
        /// It first checks whether the user is authorized as an admin by calling the <see cref="RedirectIfNotAdmin"/> method.
        /// If the user is authorized, the method checks if the tweet with the specified ID exists in the database.
        /// If the tweet exists, its details are retrieved from the database and displayed using the <see cref="View"/> method.
        /// If the tweet does not exist, a "Not Found" view is displayed using the <see cref="NotFound"/> method.
        /// If the user is not authorized, they are redirected to the appropriate page based on their login status using the <see cref="RedirectIfNotAdmin"/> method.
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            var isAuthorized = RedirectIfNotAdmin();
            if (isAuthorized != null)
            {
                return isAuthorized;
            }

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
        /// If the ModelState is valid, adds a new tweet to the database and redirects to the tweets
        /// feed page. If the ModelState is invalid, logs the validation errors, and redirects to
        /// the tweets feed page.
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

                    TempData["errorNotification"] = "An error occurred, and we were not able to process your tweet.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new tweet.");
                TempData["errorNotification"] = "An error occurred, and we were not able to process your tweet.";
                return RedirectToAction("Index", "Tweets");
            }
        }

        /// <summary>
        /// Displays a form for editing a tweet with the specified ID, if authorized as an admin.
        /// </summary>
        /// <param name="id">The ID of the tweet to edit.</param>
        /// <returns>
        /// If the user is authorized as an admin, displays a form for editing the tweet with the specified ID.
        /// If the user is not authorized as an admin, redirects the user to the appropriate page using the <see cref="RedirectIfNotAdmin"/> method.
        /// </returns>
        /// <remarks>
        /// This method displays a form for editing a tweet with the specified ID, provided that the user is authorized as an admin.
        /// It first checks whether the user is authorized as an admin by calling the <see cref="RedirectIfNotAdmin"/> method.
        /// If the user is authorized, the method checks if the tweet with the specified ID exists in the database.
        /// If the tweet exists, its details are retrieved from the database and the edit form is displayed using the <see cref="View"/> method.
        /// The form includes a <see cref="ViewData"/> element for selecting the tweeter of the tweet from a list of users.
        /// If the tweet does not exist, a "Not Found" view is displayed using the <see cref="NotFound"/> method.
        /// If the user is not authorized, they are redirected to the appropriate page based on their login status using the <see cref="RedirectIfNotAdmin"/> method.
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            var isAuthorized = RedirectIfNotAdmin();
            if (isAuthorized != null)
            {
                return isAuthorized;
            }

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

        /// <summary>
        /// Handles the submission of the edited tweet information and updates the database, if authorized as an admin.
        /// </summary>
        /// <param name="id">The ID of the tweet being edited.</param>
        /// <param name="tweets">The edited tweet information to be saved.</param>
        /// <returns>
        /// If the user is authorized as an admin and the ModelState is valid, updates the tweet information in the database and redirects to the <see cref="Index"/> action.
        /// If the user is not authorized as an admin, redirects the user to the appropriate page using the <see cref="RedirectIfNotAdmin"/> method.
        /// If the ModelState is invalid, logs validation errors and redirects to the <see cref="Index"/> action of the "Tweets" controller.
        /// If the tweet with the specified ID does not exist, returns a "Not Found" view using the <see cref="NotFound"/> method.
        /// </returns>
        /// <remarks>
        /// This method handles the submission of edited tweet information, provided that the user is authorized as an admin.
        /// It first checks whether the user is authorized as an admin by calling the <see cref="RedirectIfNotAdmin"/> method.
        /// If the user is authorized, the method compares the ID parameter with the ID property of the provided <paramref name="tweets"/> object.
        /// If they do not match, a "Not Found" view is displayed using the <see cref="NotFound"/> method.
        /// If the IDs match and the ModelState is valid, the tweet information is updated in the database using the <see cref="_context"/> object's <see cref="DbContext.Update"/> method.
        /// The updated information is saved using the <see cref="DbContext.SaveChangesAsync"/> method.
        /// If a <see cref="DbUpdateConcurrencyException"/> occurs, the method checks if the tweet still exists in the database.
        /// If the tweet does not exist, a "Not Found" view is displayed.
        /// If the tweet exists, the exception is re-thrown.
        /// If the ModelState is invalid, validation errors are logged using the <see cref="_logger"/> object, and the user is redirected to the <see cref="Index"/> action of the "Tweets" controller.
        /// If the user is not authorized, they are redirected to the appropriate page based on their login status using the <see cref="RedirectIfNotAdmin"/> method.
        /// </remarks>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Content,CreationDateTime,TweeterId")] Tweets tweets)
        {
            var isAuthorized = RedirectIfNotAdmin();
            if (isAuthorized != null)
            {
                return isAuthorized;
            }

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

        /// <summary>
        /// Displays the details of a tweet for potential deletion, if authorized as an admin.
        /// </summary>
        /// <param name="id">The ID of the tweet to be deleted.</param>
        /// <returns>
        /// If the user is authorized as an admin, displays the details of the tweet for potential deletion.
        /// If the user is not authorized as an admin, redirects the user to the appropriate page using the <see cref="RedirectIfNotAdmin"/> method.
        /// If the provided <paramref name="id"/> is null or the <see cref="TwitterCloneContext.Tweets"/> is null, returns a "Not Found" view using the <see cref="NotFound"/> method.
        /// If the tweet with the specified ID does not exist, returns a "Not Found" view using the <see cref="NotFound"/> method.
        /// </returns>
        /// <remarks>
        /// This method displays the details of a tweet for potential deletion, provided that the user is authorized as an admin.
        /// It first checks whether the user is authorized as an admin by calling the <see cref="RedirectIfNotAdmin"/> method.
        /// If the user is authorized, the method checks whether the provided <paramref name="id"/> is null or the <see cref="TwitterCloneContext.Tweets"/> is null.
        /// If either condition is met, a "Not Found" view is displayed using the <see cref="NotFound"/> method.
        /// Otherwise, the method retrieves the tweet details using the <see cref="TwitterCloneContext.Tweets"/> object's <see cref="DbContext.FindAsync"/> method
        /// and includes the tweeter information using the <see cref="Include"/> method. The retrieved tweet is displayed using the <see cref="View"/> method.
        /// If the user is not authorized, they are redirected to the appropriate page based on their login status using the <see cref="RedirectIfNotAdmin"/> method.
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            var isAuthorized = RedirectIfNotAdmin();
            if (isAuthorized != null)
            {
                return isAuthorized;
            }

            if (id == null || _context.Tweets == null)
            {
                return NotFound();
            }

            var tweets = await _context.Tweets.Include(t => t.Tweeter).FirstOrDefaultAsync(m => m.Id == id);

            if (tweets == null)
            {
                return NotFound();
            }

            return View(tweets);
        }

        /// <summary>
        /// Deletes a tweet confirmed by the user, if authorized as an admin.
        /// </summary>
        /// <param name="id">The ID of the tweet to be deleted.</param>
        /// <returns>
        /// If the user is authorized as an admin, deletes the specified tweet and redirects to the "Index" action of the "Tweets" controller.
        /// If the user is not authorized as an admin, redirects the user to the appropriate page using the <see cref="RedirectIfNotAdmin"/> method.
        /// If the <see cref="TwitterCloneContext.Tweets"/> is null, returns a "Problem" response with a specific error message.
        /// If the tweet with the specified ID does not exist, no action is taken.
        /// </returns>
        /// <remarks>
        /// This method deletes the tweet with the specified ID if the user is authorized as an admin.
        /// It first checks whether the user is authorized as an admin by calling the <see cref="RedirectIfNotAdmin"/> method.
        /// If the user is authorized, the method checks whether the <see cref="TwitterCloneContext.Tweets"/> is null.
        /// If it is null, a "Problem" response is returned with a specific error message.
        /// Otherwise, the method retrieves the tweet using the provided <paramref name="id"/> and the <see cref="TwitterCloneContext.Tweets"/> object's <see cref="DbContext.FindAsync"/> method.
        /// If the tweet is found, it is removed from the context using the <see cref="DbContext.Remove"/> method and the changes are saved using <see cref="DbContext.SaveChangesAsync"/>.
        /// Finally, the method redirects to the "Index" action of the "Tweets" controller.
        /// If the user is not authorized, they are redirected to the appropriate page based on their login status using the <see cref="RedirectIfNotAdmin"/> method.
        /// </remarks>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var isAuthorized = RedirectIfNotAdmin();
            if (isAuthorized != null)
            {
                return isAuthorized;
            }

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

        /// <summary>
        /// Retrieves a specific tweet and its related comments and displays them in a view.
        /// </summary>
        /// <param name="tweetId">The unique identifier of the tweet to retrieve.</param>
        /// <returns>An asynchronous task that represents the action result.</returns>
        [HttpGet]
        public async Task<IActionResult> TweetAndRelatedComments(int? tweetId)
        {
            try
            {
                if (_currentUserId == null)
                {
                    base._logger.LogInformation("User is not logged in. Redirecting to Home/Index.");
                    return RedirectToAction("Index", "Home");
                }

                var tweetOwnerId = _context?.Tweets?.Find(tweetId)?.TweeterId;
                TweetAndRelatedCommentsViewModel model = new TweetAndRelatedCommentsViewModel()
                {
                    Tweet = _context?.Tweets.Find(tweetId),
                    Comments = _context?.Comments.Where(c => c.OriginalTweetId == tweetId).ToList(),
                    TweetOwnerName = _context?.Users?.Find(tweetOwnerId)?.UserName,
                    TweetOwnersProfilePicture = _context?.Users?.Find(tweetOwnerId)?.ProfilePictureUrl ?? "\\default\\1.jpg"
                };

                foreach (var comment in model.Comments)
                {
                    _logger.LogInformation("Processing comment with ID {comment.Id}.", comment.Id);
                    // store the commenter's ProfilePictureUrl in the ViewData dictionary
                    ViewData[comment.Id.ToString()] = (await _context.Users.FirstOrDefaultAsync(u => u.Id == comment.CommenterId))?.ProfilePictureUrl ?? "\\default\\1.jpg";
                }

                ViewData["errorNotification"] = (string.IsNullOrEmpty(TempData["errorNotification"]?.ToString())) ? "" : TempData["errorNotification"]?.ToString();
                return View(model);
            }
            catch (Exception ex)
            {
                base._logger.LogError(ex, "Error retrieving the tweet or it's related comments");
                return RedirectToAction("Error", "Home");
            }
        }

        private bool TweetsExists(int id)
        {
            return (_context.Tweets?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}