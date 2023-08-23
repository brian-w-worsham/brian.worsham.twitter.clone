using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using worsham.twitter.clone.Models;
using worsham.twitter.clone.Services;

namespace worsham.twitter.clone.Controllers
{
    public class LikesController : Controller
    {
        private readonly TwitterCloneContext _context;
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<LikesController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LikesController(TwitterCloneContext context, IAuthenticationService authenticationService, ILogger<LikesController> logger)
        {
            _context = context;
            _authenticationService = authenticationService;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int likedTweetId) // Pass the likedTweetId from the form
        {
            if (ModelState.IsValid)
            {
                // Get the authenticated user's ID             
                int? userThatLikedTweetId = HttpContext.Session.GetInt32("UserId");
                
                _logger.LogInformation("User ID retrieved from session: {UserId}", userThatLikedTweetId);


                // Create a new Likes instance with the correct user ID and liked tweet ID
                var likes = new Likes
                {
                    LikedTweetId = likedTweetId,
                    UserThatLikedTweetId = (int)userThatLikedTweetId
                };

                _context.Add(likes);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Like created successfully for Tweet ID: {TweetId}, Liked by User ID: {UserId}", likedTweetId, userThatLikedTweetId);

                return RedirectToAction(nameof(Index));
            }

            ViewData["Id"] = new SelectList(_context.Users, "Id", "Email");
            ViewData["LikedTweetId"] = new SelectList(_context.Tweets, "Id", "Content");
            return View();
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

        // POST: Likes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
