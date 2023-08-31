using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using worsham.twitter.clone.Models.EntityModels;
using worsham.twitter.clone.Services;

namespace worsham.twitter.clone.Controllers
{
    public class FollowsController : TwitterController
    {
        private readonly TwitterCloneContext _context;
        private int? _currentUserId;

        public FollowsController(TwitterCloneContext context, ILogger<FollowsController> logger, IAuthorizationService authorizationService) : base(logger, authorizationService)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _currentUserId = HttpContext.Session.GetInt32("UserId");
            base.OnActionExecuting(context);        
        }

        /// <summary>
        /// Returns a view of all Follows.
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
            var twitterCloneContext = _context.Follows.Include(f => f.FollowedUser).Include(f => f.FollowerUser);
            return View(await twitterCloneContext.ToListAsync());
        }

        /// <summary>
        /// Returns a view of the details of the Follow with the specified id.
        /// </summary>
        /// <param name="id">The id of the Follow to view details of.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an IActionResult that represents the result of the operation.</returns>
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            var isAuthorized = RedirectIfNotAdmin();
            if (isAuthorized != null)
            {
                return isAuthorized;
            }

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

        /// <summary>
        /// Returns a view for creating a new Follow.
        /// </summary>
        /// <returns>An IActionResult that represents the result of the operation.</returns>
        [HttpGet]
        public IActionResult Create()
        {
            var isAuthorized = RedirectIfNotAdmin();
            if (isAuthorized != null)
            {
                return isAuthorized;
            }
            ViewData["FollowedUserId"] = new SelectList(_context.Users, "Id", "Email");
            ViewData["FollowerUserId"] = new SelectList(_context.Users, "Id", "Email");
            return View();
        }

        /// <summary>
        /// Creates a new follow relationship between the current user and the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user to be followed.</param>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> representing the action result after creating the
        /// follow. If successful, redirects to the "Index" action of the "Tweets" controller.
        /// </returns>
        /// <remarks>
        /// If the ModelState is valid, a new follow relationship is created and saved in the
        /// database. Logs information about the follow creation. If the ModelState is invalid or an
        /// exception occurs, appropriate error logging is performed and a redirection to the
        /// "Index" action of the "Tweets" controller is executed.
        /// </remarks>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int userId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _ = _context.Add(new Follows() { FollowedUserId = userId, FollowerUserId = (int)_currentUserId });
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Follow created: FollowerUserId = {_currentUserId}, FollowedUserId = {userId}");
                    return RedirectToAction("Index", "Tweets");
                }
                _logger.LogError("ModelState is invalid");
                TempData["errorNotification"] = "An error occurred while attempting to follow.";
                return RedirectToAction("Index", "Tweets");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating follow");
                TempData["errorNotification"] = "An error occurred while attempting to follow.";
                return RedirectToAction("Index", "Tweets");
            }
        }

        /// <summary>
        /// Returns a view of the Follow with the specified id for editing.
        /// </summary>
        /// <param name="id">The id of the Follow to edit.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an IActionResult that represents the result of the operation.</returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            var isAuthorized = RedirectIfNotAdmin();
            if (isAuthorized != null)
            {
                return isAuthorized;
            }

            if (id == null || _context.Follows == null)
            {
                return NotFound();
            }

            var follows = await _context.Follows.FindAsync(id);
            if (follows == null)
            {
                return NotFound();
            }
            ViewData["FollowedUserId"] = new SelectList(_context.Users, "Id", "Email", follows.FollowedUserId);
            ViewData["FollowerUserId"] = new SelectList(_context.Users, "Id", "Email", follows.FollowerUserId);
            return View(follows);
        }

        /// <summary>
        /// Edits a Follow with the specified id.
        /// </summary>
        /// <param name="id">The id of the Follow to edit.</param>
        /// <param name="follows">The Follow object with the updated values.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an IActionResult that represents the result of the operation.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FollowedUserId,FollowerUserId")] Follows follows)
        {
            var isAuthorized = RedirectIfNotAdmin();
            if (isAuthorized != null)
            {
                return isAuthorized;
            }

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
            ViewData["FollowedUserId"] = new SelectList(_context.Users, "Id", "Email", follows.FollowedUserId);
            ViewData["FollowerUserId"] = new SelectList(_context.Users, "Id", "Email", follows.FollowerUserId);
            return View(follows);
        }

        /// <summary>
        /// Returns a view of the Follow with the specified id for deletion.
        /// </summary>
        /// <param name="id">The id of the Follow to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an IActionResult that represents the result of the operation.</returns>
        public async Task<IActionResult> Delete(int? id)
        {
            var isAuthorized = RedirectIfNotAdmin();
            if (isAuthorized != null)
            {
                return isAuthorized;
            }

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

        /// <summary>
        /// Deletes a Follows entity based on the provided ID and redirects to a user profile page.
        /// </summary>
        /// <param name="id">The ID of the Follows entity to be deleted.</param>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> representing the action result. If successful,
        /// redirects to the "Profile" action of the "Users" controller with the FollowedUserId as a
        /// route value. If an error occurs during the operation, redirects to the "Profile" action
        /// of the "Users" controller and logs the error.
        /// </returns>
        /// <remarks>
        /// This action attempts to delete a Follows entity based on the provided ID. If the Follows
        /// entity exists, it is removed from the context. After deletion, a log message is
        /// generated. If the Follows entity does not exist, a warning log message is generated. The
        /// operation is logged. After the operation, the user is redirected to the "Profile" action
        /// of the "Users" controller with the FollowedUserId as a route value. If an exception
        /// occurs, an error log message is generated, and the user is redirected with a status code 500.
        /// </remarks>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                if (_context.Follows == null)
                {
                    _logger.LogError("Follows entity model is null");
                    TempData["errorNotification"] = "An error occurred while attempting to unfollow.";
                    return RedirectToAction("Profile", "Users");
                }

                Follows? follow = await _context.Follows.FindAsync(id);
                int? followedUserId = follow?.FollowedUserId;

                if (follow != null)
                {
                    _context.Follows.Remove(follow);
                    _logger.LogInformation($"Follow with ID {id} deleted.");
                }
                else
                {
                    _logger.LogWarning($"Attempted to delete Follow with ID {id}, but it was not found.");
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Redirecting to user profile with FollowedUserId = {followedUserId}.");
                return RedirectToAction(actionName: "Profile", controllerName: "Users", new { followedUserId = followedUserId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting a Follow.");
                TempData["errorNotification"] = "An error occurred while attempting to unfollow.";
                return RedirectToAction("Profile", "Users");
            }
        }

        // GET: api/follows/notfollowed
        /// <summary>
        /// Retrieves a list of users who are not followed by the current user, excluding the
        /// current user.
        /// </summary>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> representing the action result. If successful,
        /// returns a list of users in JSON format who are not followed by the current user. If an
        /// error occurs, returns a status code 500 along with an error message.
        /// </returns>
        /// <remarks>
        /// This endpoint queries the database for users who are not followed by the current user.
        /// It excludes the current user from the list and returns a list of users' IDs and
        /// usernames. If no errors occur during the query and processing, a log message is
        /// generated to indicate the number of users retrieved. If an exception occurs, an error
        /// log message is generated and a status code 500 response is returned with an error message.
        /// </remarks>
        [HttpGet("api/follows/notfollowed")]
        public IActionResult UsersNotFollowed()
        {
            try
            {
                // Query users who are not followed by the current user, but exclude the current
                // user from this list
                var notFollowedUsers = _context.Users
                    .Where(u => !_context.Follows.Any(f => f.FollowerUserId == _currentUserId && f.FollowedUserId == u.Id) && u.Id != _currentUserId)
                    .Select(u => new { u.Id, u.UserName }) // Return only relevant data
                    .ToList();

                _logger.LogInformation($"Retrieved {notFollowedUsers.Count} users who are not followed by the current user.");

                return Ok(notFollowedUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching users not followed.");
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }

        private bool FollowsExists(int id)
        {
            return (_context.Follows?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}