using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using worsham.twitter.clone.Models;

namespace worsham.twitter.clone.Controllers
{
    public class LikesController : Controller
    {
        private readonly TwitterCloneContext _context;

        public LikesController(TwitterCloneContext context)
        {
            _context = context;
        }

        // GET: Likes
        public async Task<IActionResult> Index()
        {
            var twitterCloneContext = _context.Likes.Include(l => l.IdNavigation).Include(l => l.LikedTweet);
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
                .Include(l => l.IdNavigation)
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

        // POST: Likes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,LikedTweetId,UserThatLikedTweetId")] Likes likes)
        {
            if (ModelState.IsValid)
            {
                _context.Add(likes);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Id"] = new SelectList(_context.Users, "Id", "Email", likes.Id);
            ViewData["LikedTweetId"] = new SelectList(_context.Tweets, "Id", "Content", likes.LikedTweetId);
            return View(likes);
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
                .Include(l => l.IdNavigation)
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
