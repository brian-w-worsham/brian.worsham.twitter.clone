using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using worsham.twitter.clone.Models.EntityModels;

namespace worsham.twitter.clone.Controllers
{
    public class CommentsController : Controller
    {
        private readonly TwitterCloneContext _context;
        private readonly ILogger<LikesController> _logger;

        public CommentsController(TwitterCloneContext context, ILogger<LikesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Comments
        public async Task<IActionResult> Index()
        {
            var twitterCloneContext = _context.Comments.Include(c => c.Commenter).Include(c => c.OriginalTweet);
            return View(await twitterCloneContext.ToListAsync());
        }

        // GET: Comments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Comments == null)
            {
                return NotFound();
            }

            var comments = await _context.Comments
                .Include(c => c.Commenter)
                .Include(c => c.OriginalTweet)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comments == null)
            {
                return NotFound();
            }

            return View(comments);
        }

        // GET: Comments/Create
        public IActionResult Create()
        {
            ViewData["CommenterId"] = new SelectList(_context.Users, "Id", "Email");
            ViewData["OriginalTweetId"] = new SelectList(_context.Tweets, "Id", "Content");
            return View();
        }

        /// <summary>
        /// Creates a new comment and adds it to the database.
        /// </summary>
        /// <param name="comments">The comment to be created.</param>
        /// <returns>
        /// If the ModelState is valid, redirects to the "Index" action of the "Tweets" controller.
        /// If the ModelState is invalid, logs validation errors, and redirects to the "Index"
        /// action of the "Tweets" controller.
        /// </returns>
        /// <remarks>
        /// If the ModelState is invalid, this method logs validation errors using the provided
        /// logger. It also contains a "Todo" comment indicating the intention to render a
        /// notification to the user in case of failure.
        /// </remarks>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Content,OriginalTweetId,CommenterId")] Comments comments)
        {
            if (ModelState.IsValid)
            {
                _context.Add(comments);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Tweets");
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
                // Todo: render a notification to the user that the comment creation failed
                return RedirectToAction("Index", "Tweets");
            }
        }

        // GET: Comments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Comments == null)
            {
                return NotFound();
            }

            var comments = await _context.Comments.FindAsync(id);
            if (comments == null)
            {
                return NotFound();
            }
            ViewData["CommenterId"] = new SelectList(_context.Users, "Id", "Email", comments.CommenterId);
            ViewData["OriginalTweetId"] = new SelectList(_context.Tweets, "Id", "Content", comments.OriginalTweetId);
            return View(comments);
        }

        // POST: Comments/Edit/5 To protect from overposting attacks, enable the specific properties
        // you want to bind to. For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Content,OriginalTweetId,CommenterId")] Comments comments)
        {
            if (id != comments.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(comments);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentsExists(comments.Id))
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
            ViewData["CommenterId"] = new SelectList(_context.Users, "Id", "Email", comments.CommenterId);
            ViewData["OriginalTweetId"] = new SelectList(_context.Tweets, "Id", "Content", comments.OriginalTweetId);
            return View(comments);
        }

        // GET: Comments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Comments == null)
            {
                return NotFound();
            }

            var comments = await _context.Comments
                .Include(c => c.Commenter)
                .Include(c => c.OriginalTweet)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comments == null)
            {
                return NotFound();
            }

            return View(comments);
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Comments == null)
            {
                return Problem("Entity set 'TwitterCloneContext.Comments'  is null.");
            }
            var comments = await _context.Comments.FindAsync(id);
            if (comments != null)
            {
                _context.Comments.Remove(comments);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CommentsExists(int id)
        {
            return (_context.Comments?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}