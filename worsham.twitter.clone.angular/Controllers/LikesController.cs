using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using worsham.twitter.clone.angular.Models;
using worsham.twitter.clone.angular.Models.EntityModels;
using worsham.twitter.clone.angular.Services;

namespace worsham.twitter.clone.angular.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("api/[controller]")]
    [ApiController]
    public class LikesController : TwitterController
    {
        private readonly TwitterCloneContext _context;

        public LikesController(TwitterCloneContext context, ILogger<LikesController> logger, IAuthorizationService authorizationService) : base(logger, authorizationService)
        {
            _context = context;
        }

        // GET: api/Likes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Likes>>> GetLikes()
        {
            if (_context.Likes == null)
            {
                return NotFound();
            }
            return await _context.Likes.ToListAsync();
        }

        // GET: api/Likes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Likes>> GetLikes(int id)
        {
            if (_context.Likes == null)
            {
                return NotFound();
            }
            var likes = await _context.Likes.FindAsync(id);

            if (likes == null)
            {
                return NotFound();
            }

            return likes;
        }

        // PUT: api/Likes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLikes(int id, Likes likes)
        {
            if (id != likes.Id)
            {
                return BadRequest();
            }

            _context.Entry(likes).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LikesExists(id))
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

        // POST: api/Likes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Likes>> PostLikes(Likes likes)
        {
            if (_context.Likes == null)
            {
                return Problem("Entity set 'TwitterCloneContext.Likes'  is null.");
            }
            _context.Likes.Add(likes);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLikes", new { id = likes.Id }, likes);
        }

        // DELETE: api/Likes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLikes(int id)
        {
            if (_context.Likes == null)
            {
                return NotFound();
            }
            var likes = await _context.Likes.FindAsync(id);
            if (likes == null)
            {
                return NotFound();
            }

            _context.Likes.Remove(likes);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Handles the HTTP POST request to like or unlike a tweet.
        /// </summary>
        /// <param name="tweetId">The ID of the tweet being liked or unliked.</param>
        /// <returns>
        /// If the ModelState is valid and the like operation succeeds, redirects to the tweets feed
        /// page. If the ModelState is invalid, renders a notification to the user in the view and
        /// redirects to the tweets feed page. If a database update exception occurs during the like
        /// operation, logs the error, renders a notification to the user, and redirects to the
        /// tweets feed page. If any other exception occurs during the like operation, logs the
        /// error and redirects to the error page.
        /// </returns>
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] int tweetId) // Pass the likedTweetId from the form
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
                    return Json(new LikeResult { Success = false, ErrorMessage = "User is not logged in." });
                }
                if (ModelState.IsValid && tweetId > 0)
                {
                    // Get the authenticated user's ID
                    int? userThatLikedTweetId = user.Id;
                    _logger.LogInformation(message: "User ID retrieved from session: {UserId}", userThatLikedTweetId?.ToString());

                    // Check if the user has already liked the tweet
                    var userHasLikedTweet = _context.Likes.Any(l => l.UserThatLikedTweetId == userThatLikedTweetId && l.LikedTweetId == tweetId);

                    if (userHasLikedTweet)
                    {
                        // remove the like from the database
                        var likeToDelete = _context?.Likes.FirstOrDefault(l => l.UserThatLikedTweetId == userThatLikedTweetId && l.LikedTweetId == tweetId);
                        if (likeToDelete == null)
                        {
                            throw new InvalidOperationException("Like to delete is null");
                        }
                        _ = _context?.Remove(entity: likeToDelete ?? null);
                    }
                    else
                    {
                        //Create a new Likes instance with the correct user ID and liked tweet ID
                        var like = new Likes
                        {
                            LikedTweetId = tweetId,
                            UserThatLikedTweetId = (int)userThatLikedTweetId
                        };

                        _context?.Add(entity: like);
                    }

                    _ = await _context?.SaveChangesAsync();

                    _logger.LogInformation(message: "Like created successfully for Tweet ID: {TweetId}, Liked by User ID: {UserId}", tweetId, userThatLikedTweetId);

                    return Json(new LikeResult { Success = true });
                }
                else
                {
                    return Json(new LikeResult { Success = false, ErrorMessage = "An error occurred while saving the like for the tweet." });
                }
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Error updating the database while creating a Like in the Create method");
                return Json(new LikeResult { Success = false, ErrorMessage = "An error occurred while saving the like for the tweet." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting creating a Like in the Create method");
                return Json(new LikeResult { Success = false, ErrorMessage = "An error occurred while saving the like for the tweet." });
            }
        }

        private bool LikesExists(int id)
        {
            return (_context.Likes?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
