using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using worsham.twitter.clone.Models.EntityModels;
using worsham.twitter.clone.Services;

namespace worsham.twitter.clone.Controllers
{
    public class ReTweetsController : TwitterController
    {
        private readonly TwitterCloneContext _context;

        public ReTweetsController(TwitterCloneContext context, ILogger<ReTweetsController> logger, IAuthorizationService authorizationService) : base(logger, authorizationService)
        {
            _context = context;
        }

        /// <summary>
        /// Returns a view of all ReTweets.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains an IActionResult that represents the result of the operation.</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var isAuthorized = RedirectIfNotAdmin();
            if (isAuthorized != null)
            {
                return isAuthorized;
            }

            var twitterCloneContext = _context.ReTweets.Include(r => r.OriginalTweet).Include(r => r.Retweeter);
            return View(await twitterCloneContext.ToListAsync());
        }

        /// <summary>
        /// Returns a view of the details of the ReTweet with the specified id.
        /// </summary>
        /// <param name="id">The id of the ReTweet to view details of.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an IActionResult that represents the result of the operation.</returns>
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            var isAuthorized = RedirectIfNotAdmin();
            if (isAuthorized != null)
            {
                return isAuthorized;
            }

            if (id == null || _context.ReTweets == null)
            {
                return NotFound();
            }

            var reTweets = await _context.ReTweets
                .Include(r => r.OriginalTweet)
                .Include(r => r.Retweeter)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reTweets == null)
            {
                return NotFound();
            }

            return View(reTweets);
        }

        /// <summary>
        /// Handles the creation or removal of retweets for a tweet by the authenticated user.
        /// </summary>
        /// <param name="tweetId">
        /// The ID of the tweet for which the retweet is being created or removed.
        /// </param>
        /// <returns>Redirects to the Tweets Index page after the retweet operation.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int tweetId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Get the authenticated user's ID
                    int? currentUserId = HttpContext.Session.GetInt32("UserId");
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

                    _logger.LogInformation("Redirecting to Tweets Index page after retweet operation.");
                    return RedirectToAction(actionName: "Index", controllerName: "Tweets");
                }
                else
                {
                    _logger.LogWarning("Model state is invalid. Validation errors: {ValidationErrors}", ModelState.Values.SelectMany(v => v.Errors));
                    TempData["errorNotification"] = "An error occurred while processing the re-tweet";
                    return RedirectToAction(actionName: "Index", controllerName: "Tweets");
                }
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Error updating the database while creating a ReTweet in the Create method");
                TempData["errorNotification"] = "An error occurred while processing the re-tweet";
                return RedirectToAction(actionName: "Index", controllerName: "Tweets");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting creating a ReTweet in the Create method");
                TempData["errorNotification"] = "An error occurred while processing the re-tweet";
                return RedirectToAction(actionName: "Index", controllerName: "Tweets");
            }
        }

        /// <summary>
        /// Returns a view of the ReTweet with the specified id for editing.
        /// </summary>
        /// <param name="id">The id of the ReTweet to edit.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an IActionResult that represents the result of the operation.</returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            var isAuthorized = RedirectIfNotAdmin();
            if (isAuthorized != null)
            {
                return isAuthorized;
            }

            if (id == null || _context.ReTweets == null)
            {
                return NotFound();
            }

            var reTweets = await _context.ReTweets.FindAsync(id);
            if (reTweets == null)
            {
                return NotFound();
            }
            ViewData["OriginalTweetId"] = new SelectList(_context.Tweets, "Id", "Content", reTweets.OriginalTweetId);
            ViewData["RetweeterId"] = new SelectList(_context.Users, "Id", "Email", reTweets.RetweeterId);
            return View(reTweets);
        }

        /// <summary>
        /// Edits a ReTweet with the specified id.
        /// </summary>
        /// <param name="id">The id of the ReTweet to edit.</param>
        /// <param name="reTweets">The ReTweet object with the updated values.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an IActionResult that represents the result of the operation.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OriginalTweetId,ReTweetCreationDateTime,RetweeterId")] ReTweets reTweets)
        {
            var isAuthorized = RedirectIfNotAdmin();
            if (isAuthorized != null)
            {
                return isAuthorized;
            }

            if (id != reTweets.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reTweets);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReTweetsExists(reTweets.Id))
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
            ViewData["OriginalTweetId"] = new SelectList(_context.Tweets, "Id", "Content", reTweets.OriginalTweetId);
            ViewData["RetweeterId"] = new SelectList(_context.Users, "Id", "Email", reTweets.RetweeterId);
            return View(reTweets);
        }

        /// <summary>
        /// Returns a view of the ReTweet with the specified id for deletion.
        /// </summary>
        /// <param name="id">The id of the ReTweet to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an IActionResult that represents the result of the operation.</returns>
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            var isAuthorized = RedirectIfNotAdmin();
            if (isAuthorized != null)
            {
                return isAuthorized;
            }

            if (id == null || _context.ReTweets == null)
            {
                return NotFound();
            }

            var reTweets = await _context.ReTweets
                .Include(r => r.OriginalTweet)
                .Include(r => r.Retweeter)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reTweets == null)
            {
                return NotFound();
            }

            return View(reTweets);
        }

        /// <summary>
        /// Deletes a ReTweet with the specified id.
        /// </summary>
        /// <param name="id">The id of the ReTweet to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an IActionResult that represents the result of the operation.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var isAuthorized = RedirectIfNotAdmin();
            if (isAuthorized != null)
            {
                return isAuthorized;
            }

            if (_context.ReTweets == null)
            {
                return Problem("Entity set 'TwitterCloneContext.ReTweets'  is null.");
            }
            var reTweets = await _context.ReTweets.FindAsync(id);
            if (reTweets != null)
            {
                _context.ReTweets.Remove(reTweets);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReTweetsExists(int id)
        {
            return (_context.ReTweets?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}