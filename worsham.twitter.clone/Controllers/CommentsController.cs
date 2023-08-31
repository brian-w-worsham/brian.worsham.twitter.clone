using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using worsham.twitter.clone.Models.EntityModels;
using worsham.twitter.clone.Services;

namespace worsham.twitter.clone.Controllers
{
    public class CommentsController : TwitterController
    {
        private readonly TwitterCloneContext _context;

        public CommentsController(TwitterCloneContext context, ILogger<CommentsController> logger, IAuthorizationService authorizationService) : base(logger, authorizationService)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _currentUserId = HttpContext.Session.GetInt32("UserId");
            _session = HttpContext.Session;
            base.OnActionExecuting(context);
        }

        /// <summary>
        /// Displays the list of comments if the user is authorized as an admin; otherwise, redirects the user.
        /// </summary>
        /// <returns>
        /// If the user is authorized as an admin, displays the list of comments in the "Index" view.
        /// If the user is not authorized as an admin, redirects the user to the appropriate page using the <see cref="RedirectIfNotAdmin"/> method.
        /// </returns>
        /// <remarks>
        /// This method checks whether the user is authorized as an admin by calling the <see cref="RedirectIfNotAdmin"/> method.
        /// If the user is authorized, the list of comments is retrieved from the database using the <see cref="_context.Comments"/> property,
        /// including related data for commenters and original tweets.
        /// The comments are then displayed in the "Index" view.
        /// If the user is not authorized, they are redirected to the appropriate page based on their login status using the <see cref="RedirectIfNotAdmin"/> method.
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var isAuthorized = RedirectIfNotAdmin();
            if (isAuthorized != null)
            {
                return isAuthorized;
            }

            var twitterCloneContext = _context.Comments.Include(c => c.Commenter).Include(c => c.OriginalTweet);
            return View(await twitterCloneContext.ToListAsync());
        }

        /// <summary>
        /// Displays the details of a comment if the user is authorized as an admin; otherwise, redirects the user.
        /// </summary>
        /// <param name="id">The ID of the comment to display details for.</param>
        /// <returns>
        /// If the user is authorized as an admin and the comment exists, displays the details of the comment in the "Details" view.
        /// If the user is authorized but the comment does not exist, returns a <see cref="NotFoundResult"/>.
        /// If the user is not authorized as an admin, redirects the user to the appropriate page using the <see cref="RedirectIfNotAdmin"/> method.
        /// </returns>
        /// <remarks>
        /// This method checks whether the user is authorized as an admin by calling the <see cref="RedirectIfNotAdmin"/> method.
        /// If the user is authorized, the method attempts to retrieve the comment with the specified ID from the database using the <see cref="_context.Comments"/> property,
        /// including related data for the commenter and original tweet.
        /// If the comment is found, its details are displayed in the "Details" view.
        /// If the comment does not exist, a <see cref="NotFoundResult"/> is returned.
        /// If the user is not authorized, they are redirected to the appropriate page based on their login status using the <see cref="RedirectIfNotAdmin"/> method.
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            var isAuthorized = RedirectIfNotAdmin();
            if (isAuthorized != null)
            {
                return isAuthorized;
            }

            if (id == null || _context.Comments == null)
            {
                return NotFound();
            }

            var comments = await _context.Comments.Include(c => c.Commenter).Include(c => c.OriginalTweet).FirstOrDefaultAsync(m => m.Id == id);
            if (comments == null)
            {
                return NotFound();
            }

            return View(comments);
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
        /// logger.
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
                TempData["errorNotification"] = "Comment creation failed.";
                return RedirectToAction("Index", "Tweets");
            }
        }

        /// <summary>
        /// Displays the edit page for a comment if the user is authorized as an admin; otherwise, redirects the user.
        /// </summary>
        /// <param name="id">The ID of the comment to edit.</param>
        /// <returns>
        /// If the user is authorized as an admin and the comment exists, displays the edit page for the comment in the "Edit" view.
        /// If the user is authorized but the comment does not exist, returns a <see cref="NotFoundResult"/>.
        /// If the user is not authorized as an admin, redirects the user to the appropriate page using the <see cref="RedirectIfNotAdmin"/> method.
        /// </returns>
        /// <remarks>
        /// This method checks whether the user is authorized as an admin by calling the <see cref="RedirectIfNotAdmin"/> method.
        /// If the user is authorized, the method attempts to retrieve the comment with the specified ID from the database using the <see cref="_context.Comments"/> property.
        /// If the comment is found, the method prepares the necessary data for the edit view, including the list of users and tweets for drop-down lists.
        /// The edit page for the comment is then displayed in the "Edit" view.
        /// If the comment does not exist, a <see cref="NotFoundResult"/> is returned.
        /// If the user is not authorized, they are redirected to the appropriate page based on their login status using the <see cref="RedirectIfNotAdmin"/> method.
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            var isAuthorized = RedirectIfNotAdmin();
            if (isAuthorized != null)
            {
                return isAuthorized;
            }

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

        /// <summary>
        /// Handles the submission of edited comment data and updates the comment if authorized as an admin.
        /// </summary>
        /// <param name="id">The ID of the comment to edit.</param>
        /// <param name="comments">The edited comment data to update.</param>
        /// <returns>
        /// If the user is authorized as an admin, updates the comment data in the database and redirects to the "Index" action of the "Comments" controller.
        /// If the user is authorized but the comment does not exist, returns a <see cref="NotFoundResult"/>.
        /// If the user is not authorized as an admin, redirects the user to the appropriate page using the <see cref="RedirectIfNotAdmin"/> method.
        /// If the ModelState is invalid, prepares the necessary data for the edit view, including the list of users and tweets for drop-down lists, and returns the edit page for the comment in the "Edit" view.
        /// </returns>
        /// <remarks>
        /// This method handles the submission of edited comment data from the edit view.
        /// It first checks whether the user is authorized as an admin by calling the <see cref="RedirectIfNotAdmin"/> method.
        /// If the user is authorized, the method checks if the comment with the specified ID exists.
        /// If the comment does not exist, a <see cref="NotFoundResult"/> is returned.
        /// If the comment exists, the method updates the comment data in the database and redirects to the "Index" action of the "Comments" controller.
        /// If the user is not authorized, they are redirected to the appropriate page based on their login status using the <see cref="RedirectIfNotAdmin"/> method.
        /// If the ModelState is invalid (i.e., there are validation errors in the submitted data), the method prepares the necessary data for the edit view,
        /// including the list of users and tweets for drop-down lists, and returns the edit page for the comment in the "Edit" view.
        /// </remarks>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Content,OriginalTweetId,CommenterId")] Comments comments)
        {
            var isAuthorized = RedirectIfNotAdmin();
            if (isAuthorized != null)
            {
                return isAuthorized;
            }

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

        /// <summary>
        /// Displays the details of a comment for deletion, if authorized as an admin.
        /// </summary>
        /// <param name="id">The ID of the comment to delete.</param>
        /// <returns>
        /// If the user is authorized as an admin, displays the details of the comment for deletion in the "Delete" view.
        /// If the user is authorized but the comment does not exist, returns a <see cref="NotFoundResult"/>.
        /// If the user is not authorized as an admin, redirects the user to the appropriate page using the <see cref="RedirectIfNotAdmin"/> method.
        /// </returns>
        /// <remarks>
        /// This method handles the display of the details of a comment for deletion.
        /// It first checks whether the user is authorized as an admin by calling the <see cref="RedirectIfNotAdmin"/> method.
        /// If the user is authorized, the method checks if the comment with the specified ID exists.
        /// If the comment does not exist, a <see cref="NotFoundResult"/> is returned.
        /// If the comment exists, the method retrieves the comment details from the database and displays them for deletion in the "Delete" view.
        /// If the user is not authorized, they are redirected to the appropriate page based on their login status using the <see cref="RedirectIfNotAdmin"/> method.
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            var isAuthorized = RedirectIfNotAdmin();
            if (isAuthorized != null)
            {
                return isAuthorized;
            }

            if (id == null || _context.Comments == null)
            {
                return NotFound();
            }

            var comments = await _context.Comments.Include(c => c.Commenter).Include(c => c.OriginalTweet).FirstOrDefaultAsync(m => m.Id == id);

            if (comments == null)
            {
                return NotFound();
            }

            return View(comments);
        }

        /// <summary>
        /// Deletes a comment from the database after confirmation, if authorized as an admin.
        /// </summary>
        /// <param name="id">The ID of the comment to delete.</param>
        /// <returns>
        /// If the user is authorized as an admin, deletes the comment from the database and redirects to the "Index" action of the "Comments" controller.
        /// If the user is not authorized as an admin, redirects the user to the appropriate page using the <see cref="RedirectIfNotAdmin"/> method.
        /// </returns>
        /// <remarks>
        /// This method handles the deletion of a comment from the database after confirmation.
        /// It first checks whether the user is authorized as an admin by calling the <see cref="RedirectIfNotAdmin"/> method.
        /// If the user is authorized, the method checks if the comment with the specified ID exists in the database.
        /// If the comment exists, it is removed from the database and changes are saved using the <see cref="TwitterCloneContext.SaveChangesAsync"/> method.
        /// After successful deletion, the method redirects to the "Index" action of the "Comments" controller.
        /// If the user is not authorized, they are redirected to the appropriate page based on their login status using the <see cref="RedirectIfNotAdmin"/> method.
        /// </remarks>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var isAuthorized = RedirectIfNotAdmin();
            if (isAuthorized != null)
            {
                return isAuthorized;
            }

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