using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using worsham.twitter.clone.Models.EntityModels;

namespace worsham.twitter.clone.Controllers
{
    public class LikesController : Controller
    {
        private readonly TwitterCloneContext _context;
        private readonly ILogger<LikesController> _logger;

        public LikesController(TwitterCloneContext context, ILogger<LikesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Likes
        public async Task<IActionResult> Index()
        {
            var twitterCloneContext = _context.Likes.Include(l => l.UserThatLikedTweet).Include(l => l.LikedTweet);
            return View(await twitterCloneContext.ToListAsync());
        }

        // GET: Likes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Likes == null)
            {
                return NotFound();
            }

            var likes = await _context.Likes
                .Include(l => l.UserThatLikedTweet)
                .Include(l => l.LikedTweet)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (likes == null)
            {
                return NotFound();
            }

            return View(likes);
        }

        // GET: Likes/Create
        public IActionResult Create()
        {
            ViewData["Id"] = new SelectList(_context.Users, "Id", "Email");
            ViewData["LikedTweetId"] = new SelectList(_context.Tweets, "Id", "Content");
            return View();
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int tweetId) // Pass the likedTweetId from the form
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Get the authenticated user's ID
                    int? userThatLikedTweetId = HttpContext.Session.GetInt32("UserId");
                    _logger.LogInformation(message: "User ID retrieved from session: {UserId}", userThatLikedTweetId);

                    // Check if the user has already liked the tweet
                    var userHasLikedTweet = _context.Likes.Any(l => l.UserThatLikedTweetId == userThatLikedTweetId && l.LikedTweetId == tweetId);

                    if (userHasLikedTweet)
                    {
                        // remove the like from the database
                        var likeToRemove = _context.Likes.FirstOrDefault(l => l.UserThatLikedTweetId == userThatLikedTweetId && l.LikedTweetId == tweetId);
                        _ = _context?.Remove(entity: likeToRemove);
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

                    return RedirectToAction(actionName: "Index", controllerName: "Tweets");
                }
                else
                {
                    TempData["errorNotification"] = "An error occurred while saving the like for the tweet.";
                    return RedirectToAction(actionName: "Index", controllerName: "Tweets");
                }
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Error updating the database while creating a Like in the Create method");
                TempData["errorNotification"] = "An error occurred while saving the like for the tweet.";
                return RedirectToAction(actionName: "Index", controllerName: "Tweets");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting creating a Like in the Create method");
                TempData["errorNotification"] = "An error occurred while saving the like for the tweet.";
                return RedirectToAction(actionName: "Index", controllerName: "Tweets");
            }
        }

        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        // GET: Likes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Likes == null)
            {
                return NotFound();
            }

            var likes = await _context.Likes.FindAsync(id);
            if (likes == null)
            {
                return NotFound();
            }
            ViewData["Id"] = new SelectList(_context.Users, "Id", "Email", likes.Id);
            ViewData["LikedTweetId"] = new SelectList(_context.Tweets, "Id", "Content", likes.LikedTweetId);
            return View(likes);
        }

        // POST: Likes/Edit/5 To protect from overposting attacks, enable the specific properties
        // you want to bind to. For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,LikedTweetId,UserThatLikedTweetId")] Likes likes)
        {
            if (id != likes.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(likes);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LikesExists(likes.Id))
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
            ViewData["Id"] = new SelectList(_context.Users, "Id", "Email", likes.Id);
            ViewData["LikedTweetId"] = new SelectList(_context.Tweets, "Id", "Content", likes.LikedTweetId);
            return View(likes);
        }

        // GET: Likes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Likes == null)
            {
                return NotFound();
            }

            var likes = await _context.Likes
                .Include(l => l.UserThatLikedTweet)
                .Include(l => l.LikedTweet)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (likes == null)
            {
                return NotFound();
            }

            return View(likes);
        }

        // POST: Likes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Likes == null)
            {
                return Problem("Entity set 'TwitterCloneContext.Likes'  is null.");
            }
            var likes = await _context.Likes.FindAsync(id);
            if (likes != null)
            {
                _context.Likes.Remove(likes);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LikesExists(int id)
        {
            return (_context.Likes?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}