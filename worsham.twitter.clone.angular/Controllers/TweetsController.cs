using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using worsham.twitter.clone.angular.Models;
using worsham.twitter.clone.angular.Models.EntityModels;
using worsham.twitter.clone.angular.Services;
using IAuthorizationService = worsham.twitter.clone.angular.Services.IAuthorizationService;

namespace worsham.twitter.clone.angular.Controllers
{
    [EnableCors("AllowOrigin")]
    [ApiController]
    [Route("api/[controller]")]
    public class TweetsController : TwitterController
    {
        private readonly TwitterCloneContext _context;
        private int? _currentUserId;
        private readonly IConfiguration _configuration;

        public TweetsController(TwitterCloneContext context, ILogger<TweetsController> logger, IAuthorizationService authorizationService, IConfiguration configuration) : base(logger, authorizationService)
        {
            _context = context;
            _configuration = configuration;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _currentUserId = HttpContext.Session.GetInt32("UserId");
            base.OnActionExecuting(context);
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
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] PostModel postModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Retrieve the JWT token from the Authorization header
                    var authorizationHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
                    if (authorizationHeader == null)
                    {
                        throw new ArgumentNullException(nameof(authorizationHeader));
                    }
                    var user = await _authorizationService.GetAuthenticatedUserAsync(authorizationHeader);

                    _ = _context.Add(new Tweets()
                    {
                        Content = postModel.Content,
                        CreationDateTime = DateTime.UtcNow,
                        TweeterId = user.Id
                    });
                    await _context.SaveChangesAsync();
                    // return Ok();
                    return Json(new TwitterApiActionResult { Success = true });
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

                    return Json(new TwitterApiActionResult { Success = false, ErrorMessage = "An error occurred, and we were not able to process your tweet." });
                }
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogError(ex, "Invalid JWT token.");
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new tweet.");
                return Json(new TwitterApiActionResult { Success = false, ErrorMessage = "An error occurred, and we were not able to process your tweet." });
            }
        }

        /// <summary>
        /// Retrieves returns the user's feed of tweets and retweets
        /// </summary>
        /// <returns>
        /// An IActionResult representing the Index page with the user's tweet feed.
        /// </returns>
        [HttpGet("get_tweets_feed")]
        public async Task<ActionResult<TweetsFeedViewModel>> GetTweetsFeed()
        {
            try
            {
                // Retrieve the JWT token from the Authorization header
                var authorizationHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
                if (authorizationHeader == null)
                {
                    throw new ArgumentNullException(nameof(authorizationHeader));
                }
                var user = await _authorizationService.GetAuthenticatedUserAsync(authorizationHeader);

                if (user.Id < 1)
                {
                    _logger.LogInformation("User is not logged in.");
                    return Json(new TwitterApiActionResult { Success = false, ErrorMessage = "User is not logged in." });
                }
                _currentUserId = user.Id;
                var followedUserIds = GetFollowedUserIds();

                var tweets = await GetTweetsAsync(followedUserIds);
                var reTweets = await GetReTweetsAsync(followedUserIds);

                var combinedAndSortedTweets = CombineAndSortTweets(tweets, reTweets);

                _logger.LogInformation("Combined and sorted {Count} tweets and retweets.", combinedAndSortedTweets.Count);
                _logger.LogInformation("Number of followed users: {FollowedUserCount}", followedUserIds.Count());

                var tweetModels = await CreateTweetModels(combinedAndSortedTweets);

                tweetModels.Sort((item1, item2) => item1.TimeSincePosted.CompareTo(item2.TimeSincePosted));

                string errorNotification = TempData["errorNotification"]?.ToString() ?? "";

                var tweetsFeedViewModel = new TweetsFeedViewModel(
                    HasErrors: false,
                    ValidationErrors: Enumerable.Empty<string>(),
                    Tweets: tweetModels,
                    Post: new PostModel(),
                    currentUserId: _currentUserId,
                    ErrorNotification: errorNotification
                );

                var json = JsonSerializer.Serialize<TweetsFeedViewModel>(tweetsFeedViewModel, new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.Preserve,
                    IncludeFields = true,
                    WriteIndented = true,
                    MaxDepth = 128
                });

                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tweets in the Index method");
                return Json(new TwitterApiActionResult { Success = false, ErrorMessage = "An error occurred while retrieving your tweets." });
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
                    TimeAgo = GetTimeAgo(DateTime.UtcNow - tweet.CreationDateTime),
                    Content = tweet.Content,
                    TweeterUserId = tweet.ReTweets.Any() ? tweet.ReTweets.First().OriginalTweet.TweeterId : tweet.TweeterId,
                    TweeterUserName = tweeter?.UserName,
                    Likes = tweet.Likes.ToList(),
                    Comments = tweet.Comments.ToList(),
                    Retweets = tweet.ReTweets.ToList(),
                    TweeterProfilePictureUrl = tweeter?.ProfilePictureUrl ?? "assets\\images\\uploads\\profile_pictures\\default\\1.jpg"
                });
            }
            return tweetModels;
        }

        /// <summary>
        /// Returns a string representation of the time elapsed since the given TimeSpan.
        /// </summary>
        /// <param name="timeSpan">The TimeSpan to convert to a string representation of elapsed time.</param>
        /// <returns>A string representation of the time elapsed since the given TimeSpan.</returns>
        private string GetTimeAgo(TimeSpan timeSpan)
        {
            if (timeSpan.TotalSeconds < 1)
            {
                return "just now";
            }
            if (timeSpan.TotalMinutes < 1)
            {
                return $"{timeSpan.Seconds}s";
            }
            if (timeSpan.TotalHours < 1)
            {
                return $"{timeSpan.Minutes}m";
            }
            if (timeSpan.TotalDays < 1)
            {
                return $"{timeSpan.Hours}h";
            }
            if (timeSpan.TotalDays < 30)
            {
                return $"{timeSpan.Days}d";
            }
            if (timeSpan.TotalDays < 365)
            {
                return $"{timeSpan.Days / 30}mo";
            }
            else
            {
                return $"{timeSpan.Days / 365}y";
            }
        }

        [HttpGet("tweet_and_related_comments/{tweetId}")]
        public async Task<IActionResult> TweetAndRelatedComments(int tweetId)
        {
            try
            {
                var authorizationHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
                if (authorizationHeader == null)
                {
                    throw new ArgumentNullException(nameof(authorizationHeader));
                }
                var user = await _authorizationService.GetAuthenticatedUserAsync(authorizationHeader);

                if (user.Id < 1)
                {
                    _logger.LogInformation("User is not logged in.");
                    return Json(new TwitterApiActionResult { Success = false, ErrorMessage = "User is not logged in." });
                }
                _currentUserId = user.Id;

                var tweet = await _context.Tweets.FindAsync(tweetId);
                if (tweet == null)
                {
                    // Handle tweet not found scenario if required
                    return NotFound();
                }

                var tweetOwner = await _context.Users.FindAsync(tweet.TweeterId);

                TweetAndRelatedCommentsViewModel model = new TweetAndRelatedCommentsViewModel()
                {
                    TweetOwnerName = tweetOwner?.UserName,
                    TweetOwnersProfilePicture = tweetOwner?.ProfilePictureUrl ?? "assets\\images\\uploads\\profile_pictures\\default\\1.jpg",
                    TweetContent = tweet.Content,
                    TweetCreationDateTime = tweet.CreationDateTime,
                    TweetComments = _context.Comments.Where(c => c.OriginalTweetId == tweetId).Join(_context.Users, comment => comment.CommenterId, user => user.Id, (comment, user) => new CommentModelView()
                    {
                        CommentId = comment.Id,
                        CommenterId = comment.CommenterId,
                        CommenterUserName = user.UserName,
                        CommentersProfilePicture = user.ProfilePictureUrl ?? "assets\\images\\uploads\\profile_pictures\\default\\1.jpg",
                        CommentContent = comment.Content
                    }).ToList()
                };

                var json = JsonSerializer.Serialize<TweetAndRelatedCommentsViewModel>(model, new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.Preserve,
                    IncludeFields = true,
                    WriteIndented = true,
                    MaxDepth = 256,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving the tweet or it's related comments");
                return Json(new TwitterApiActionResult { Success = false, ErrorMessage = "Error retrieving the tweet or it's related comments" });
            }
        }
    }
}
