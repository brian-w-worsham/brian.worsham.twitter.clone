using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using worsham.twitter.clone.Models;
using worsham.twitter.clone.Models.EntityModels;

namespace worsham.twitter.clone.Controllers
{
    public class FollowsController : Controller
    {
        private readonly TwitterCloneContext _context;
        private readonly ILogger<LikesController> _logger;
        private int? _currentUserId;

        public FollowsController(TwitterCloneContext context, ILogger<LikesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _currentUserId = HttpContext.Session.GetInt32("UserId");
            base.OnActionExecuting(context);
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
            ViewData["FollowedUserId"] = new SelectList(_context.Users, "Id", "Email");
            ViewData["FollowerUserId"] = new SelectList(_context.Users, "Id", "Email");
            return View();
        }

        /// <summary>
        /// Creates a new follow relationship between the current user and the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user to be followed.</param>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> representing the action result after creating the follow.
        /// If successful, redirects to the "Index" action of the "Tweets" controller.
        /// </returns>
        /// <remarks>
        /// If the ModelState is valid, a new follow relationship is created and saved in the database.
        /// Logs information about the follow creation. If the ModelState is invalid or an exception occurs,
        /// appropriate error logging is performed and a redirection to the "Index" action of the "Tweets" controller is executed.
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
                //Todo: return a notifcation to the user that the follow was not created
                return RedirectToAction("Index", "Tweets");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating follow");
                //Todo: return a notifcation to the user that the follow was not created
                return RedirectToAction("Index", "Tweets");
            }
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
            ViewData["FollowedUserId"] = new SelectList(_context.Users, "Id", "Email", follows.FollowedUserId);
            ViewData["FollowerUserId"] = new SelectList(_context.Users, "Id", "Email", follows.FollowerUserId);
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
            ViewData["FollowedUserId"] = new SelectList(_context.Users, "Id", "Email", follows.FollowedUserId);
            ViewData["FollowerUserId"] = new SelectList(_context.Users, "Id", "Email", follows.FollowerUserId);
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

        /// <summary>
        /// Deletes a Follows entity based on the provided ID and redirects to a user profile page.
        /// </summary>
        /// <param name="id">The ID of the Follows entity to be deleted.</param>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> representing the action result.
        /// If successful, redirects to the "Profile" action of the "Users" controller with the FollowedUserId as a route value.
        /// If an error occurs during the operation, redirects to the "Profile" action of the "Users" controller and logs the error.
        /// </returns>
        /// <remarks>
        /// This action attempts to delete a Follows entity based on the provided ID. If the Follows entity exists,
        /// it is removed from the context. After deletion, a log message is generated. If the Follows entity
        /// does not exist, a warning log message is generated. The operation is logged. After the operation,
        /// the user is redirected to the "Profile" action of the "Users" controller with the FollowedUserId as a route value.
        /// If an exception occurs, an error log message is generated, and the user is redirected with a status code 500.
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
                    // Todo: return a notification to the user that the unfollow failed
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
                // Todo: return a notification to the user that the unfollow failed
                return RedirectToAction("Profile", "Users");
            }
        }


        // GET: api/follows/notfollowed
        /// <summary>
        /// Retrieves a list of users who are not followed by the current user, excluding the current user.
        /// </summary>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> representing the action result.
        /// If successful, returns a list of users in JSON format who are not followed by the current user.
        /// If an error occurs, returns a status code 500 along with an error message.
        /// </returns>
        /// <remarks>
        /// This endpoint queries the database for users who are not followed by the current user.
        /// It excludes the current user from the list and returns a list of users' IDs and usernames.
        /// If no errors occur during the query and processing, a log message is generated to indicate
        /// the number of users retrieved. If an exception occurs, an error log message is generated
        /// and a status code 500 response is returned with an error message.
        /// </remarks>
        [HttpGet("api/follows/notfollowed")]
        public IActionResult UsersNotFollowed()
        {
            try
            {
                // Query users who are not followed by the current user, but exclude the current user from this list
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
