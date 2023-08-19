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
    public class ReTweetsController : Controller
    {
        private readonly TwitterCloneContext _context;

        public ReTweetsController(TwitterCloneContext context)
        {
            _context = context;
        }

        // GET: ReTweets
        public async Task<IActionResult> Index()
        {
            var twitterCloneContext = _context.ReTweets.Include(r => r.OriginalTweet).Include(r => r.Retweeter);
            return View(await twitterCloneContext.ToListAsync());
        }

        // GET: ReTweets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
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

        // GET: ReTweets/Create
        public IActionResult Create()
        {
            ViewData["OriginalTweetId"] = new SelectList(_context.Tweets, "Id", "Content");
            ViewData["RetweeterId"] = new SelectList(_context.Users, "Id", "Email");
            return View();
        }

        // POST: ReTweets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,OriginalTweetId,ReTweetCreationDateTime,RetweeterId")] ReTweets reTweets)
        {
            if (ModelState.IsValid)
            {
                _context.Add(reTweets);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["OriginalTweetId"] = new SelectList(_context.Tweets, "Id", "Content", reTweets.OriginalTweetId);
            ViewData["RetweeterId"] = new SelectList(_context.Users, "Id", "Email", reTweets.RetweeterId);
            return View(reTweets);
        }

        // GET: ReTweets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
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

        // POST: ReTweets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OriginalTweetId,ReTweetCreationDateTime,RetweeterId")] ReTweets reTweets)
        {
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

        // GET: ReTweets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
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

        // POST: ReTweets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
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
