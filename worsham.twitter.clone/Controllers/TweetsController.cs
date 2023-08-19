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
    public class TweetsController : Controller
    {
        private readonly TwitterCloneContext _context;

        public TweetsController(TwitterCloneContext context)
        {
            _context = context;
        }

        // GET: Tweets
        public async Task<IActionResult> Index()
        {
            var twitterCloneContext = _context.Tweets.Include(t => t.Tweeter);
            return View(await twitterCloneContext.ToListAsync());
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
