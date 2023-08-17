using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using brian.worsham.twitter.clone2.Data;
using brian.worsham.twitter.clone2.Models;

namespace brian.worsham.twitter.clone2.Controllers
{
    public class FollowsController : Controller
    {
        private readonly TwitterCloneContext _context;

        public FollowsController(TwitterCloneContext context)
        {
            _context = context;
        }

        // GET: Follows
        public async Task<IActionResult> Index()
        {
            var twitterCloneContext = _context.Follows.Include(f => f.FollowedUser).Include(f => f.FollowerUser);
            return View(await twitterCloneContext.ToListAsync());
        }

        // GET: Follows/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Follows == null)
            {
                return NotFound();
            }

            var follows = await _context.Follows
                .Include(f => f.FollowedUser)
                .Include(f => f.FollowerUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (follows == null)
            {
                return NotFound();
            }

            return View(follows);
        }

        // GET: Follows/Create
        public IActionResult Create()
        {
            ViewData["FollowedUserId"] = new SelectList(_context.AspNetUsers, "Id", "Id");
            ViewData["FollowerUserId"] = new SelectList(_context.AspNetUsers, "Id", "Id");
            return View();
        }

        // POST: Follows/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FollowedUserId,FollowerUserId")] Follows follows)
        {
            if (ModelState.IsValid)
            {
                _context.Add(follows);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FollowedUserId"] = new SelectList(_context.AspNetUsers, "Id", "Id", follows.FollowedUserId);
            ViewData["FollowerUserId"] = new SelectList(_context.AspNetUsers, "Id", "Id", follows.FollowerUserId);
            return View(follows);
        }

        // GET: Follows/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Follows == null)
            {
                return NotFound();
            }

            var follows = await _context.Follows.FindAsync(id);
            if (follows == null)
            {
                return NotFound();
            }
            ViewData["FollowedUserId"] = new SelectList(_context.AspNetUsers, "Id", "Id", follows.FollowedUserId);
            ViewData["FollowerUserId"] = new SelectList(_context.AspNetUsers, "Id", "Id", follows.FollowerUserId);
            return View(follows);
        }

        // POST: Follows/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FollowedUserId,FollowerUserId")] Follows follows)
        {
            if (id != follows.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(follows);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FollowsExists(follows.Id))
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
            ViewData["FollowedUserId"] = new SelectList(_context.AspNetUsers, "Id", "Id", follows.FollowedUserId);
            ViewData["FollowerUserId"] = new SelectList(_context.AspNetUsers, "Id", "Id", follows.FollowerUserId);
            return View(follows);
        }

        // GET: Follows/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Follows == null)
            {
                return NotFound();
            }

            var follows = await _context.Follows
                .Include(f => f.FollowedUser)
                .Include(f => f.FollowerUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (follows == null)
            {
                return NotFound();
            }

            return View(follows);
        }

        // POST: Follows/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Follows == null)
            {
                return Problem("Entity set 'TwitterCloneContext.Follows'  is null.");
            }
            var follows = await _context.Follows.FindAsync(id);
            if (follows != null)
            {
                _context.Follows.Remove(follows);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FollowsExists(int id)
        {
          return (_context.Follows?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
