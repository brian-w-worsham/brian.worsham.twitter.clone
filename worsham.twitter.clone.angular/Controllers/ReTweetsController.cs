using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using worsham.twitter.clone.angular.Models;
using worsham.twitter.clone.angular.Models.EntityModels;
using worsham.twitter.clone.angular.Services;

namespace worsham.twitter.clone.angular.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReTweetsController : TwitterController
    {
        private readonly TwitterCloneContext _context;

        public ReTweetsController(TwitterCloneContext context, ILogger<ReTweetsController> logger, IAuthorizationService authorizationService) : base(logger, authorizationService)
        {
            _context = context;
        }

        // GET: api/ReTweets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReTweets>>> GetReTweets()
        {
          if (_context.ReTweets == null)
          {
              return NotFound();
          }
            return await _context.ReTweets.ToListAsync();
        }

        // GET: api/ReTweets/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReTweets>> GetReTweets(int id)
        {
          if (_context.ReTweets == null)
          {
              return NotFound();
          }
            var reTweets = await _context.ReTweets.FindAsync(id);

            if (reTweets == null)
            {
                return NotFound();
            }

            return reTweets;
        }

        // PUT: api/ReTweets/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReTweets(int id, ReTweets reTweets)
        {
            if (id != reTweets.Id)
            {
                return BadRequest();
            }

            _context.Entry(reTweets).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReTweetsExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ReTweets
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ReTweets>> PostReTweets(ReTweets reTweets)
        {
          if (_context.ReTweets == null)
          {
              return Problem("Entity set 'TwitterCloneContext.ReTweets'  is null.");
          }
            _context.ReTweets.Add(reTweets);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetReTweets", new { id = reTweets.Id }, reTweets);
        }

        // DELETE: api/ReTweets/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReTweets(int id)
        {
            if (_context.ReTweets == null)
            {
                return NotFound();
            }
            var reTweets = await _context.ReTweets.FindAsync(id);
            if (reTweets == null)
            {
                return NotFound();
            }

            _context.ReTweets.Remove(reTweets);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Handles the creation or removal of retweets for a tweet by the authenticated user.
        /// </summary>
        /// <param name="tweetId">
        /// The ID of the tweet for which the retweet is being created or removed.
        /// </param>
        /// <returns>Redirects to the Tweets Index page after the retweet operation.</returns>
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] int tweetId)
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

                if (ModelState.IsValid && tweetId > 0)
                {
                    // Get the authenticated user's ID
                    int? currentUserId = user.Id;
                    _logger.LogInformation(message: "User ID retrieved from session: {UserId}", currentUserId);

                    // Check if the user has already retweeted the tweet
                    bool userHasRetweetedTweet = _context.ReTweets.Any(predicate: retweet => retweet.RetweeterId == currentUserId && retweet.OriginalTweetId == tweetId);
                    _logger.LogInformation("Retweet operation: {Operation}", userHasRetweetedTweet ? "Remove" : "Add");

                    if (userHasRetweetedTweet)
                    {
                        // remove the retweet from the database
                        var retweetToRemove = _context.ReTweets.FirstOrDefault(predicate: retweet => retweet.RetweeterId == currentUserId && retweet.OriginalTweetId == tweetId);
                        _ = _context?.Remove(entity: retweetToRemove);
                    }
                    else
                    {
                        //Create a new ReTweets instance with the correct user ID and retweeted tweet ID
                        ReTweets? retweet = new ReTweets
                        {
                            OriginalTweetId = tweetId,
                            RetweeterId = (int)currentUserId,
                            ReTweetCreationDateTime = DateTime.UtcNow
                        };

                        // Add the new retweet to the database
                        _ = _context?.Add(entity: retweet);
                    }

                    // Save the changes to the database
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Retweet {Operation} successfully for Tweet ID: {TweetId}, Retweeted by User ID: {UserId}", userHasRetweetedTweet ? "removed" : "created", tweetId, currentUserId);
                    return Json(new TwitterApiActionResult { Success = true });
                }
                else
                {
                    _logger.LogWarning("Model state is invalid. Validation errors: {ValidationErrors}", ModelState.Values.SelectMany(v => v.Errors));
                    return Json(new TwitterApiActionResult { Success = false, ErrorMessage = "An error occurred while processing the re-tweet" });
                }
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Error updating the database while creating a ReTweet in the Create method");
                return Json(new TwitterApiActionResult { Success = false, ErrorMessage = "An error occurred while processing the re-tweet" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting creating a ReTweet in the Create method");
                return Json(new TwitterApiActionResult { Success = false, ErrorMessage = "An error occurred while processing the re-tweet" });
            }
        }

        private bool ReTweetsExists(int id)
        {
            return (_context.ReTweets?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
