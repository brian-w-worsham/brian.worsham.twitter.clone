using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using worsham.twitter.clone.angular.Models;
using worsham.twitter.clone.angular.Models.EntityModels;
using worsham.twitter.clone.angular.Services;
using IAuthorizationService = worsham.twitter.clone.angular.Services.IAuthorizationService;

namespace worsham.twitter.clone.angular.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FollowsController : TwitterController
    {
        private readonly TwitterCloneContext _context;
        private int? _currentUserId;

        public FollowsController(TwitterCloneContext context, ILogger<FollowsController> logger, IAuthorizationService authorizationService) : base(logger, authorizationService)
        {
            _context = context;
        }

        // GET: api/Follows
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Follows>>> GetFollows()
        {
            if (_context.Follows == null)
            {
                return NotFound();
            }
            return await _context.Follows.ToListAsync();
        }

        // GET: api/Follows/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Follows>> GetFollows(int id)
        {
            if (_context.Follows == null)
            {
                return NotFound();
            }
            var follows = await _context.Follows.FindAsync(id);

            if (follows == null)
            {
                return NotFound();
            }

            return follows;
        }

        // PUT: api/Follows/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFollows(int id, Follows follows)
        {
            if (id != follows.Id)
            {
                return BadRequest();
            }

            _context.Entry(follows).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FollowsExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Follows
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Follows>> PostFollows(Follows follows)
        {
            if (_context.Follows == null)
            {
                return Problem("Entity set 'TwitterCloneContext.Follows'  is null.");
            }
            _context.Follows.Add(follows);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFollows", new { id = follows.Id }, follows);
        }

        // DELETE: api/Follows/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFollows(int id)
        {
            if (_context.Follows == null)
            {
                return NotFound();
            }
            var follows = await _context.Follows.FindAsync(id);
            if (follows == null)
            {
                return NotFound();
            }

            _context.Follows.Remove(follows);
            await _context.SaveChangesAsync();

            return NoContent();
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
        [HttpGet("notfollowed")]
        public async Task<IActionResult> UsersNotFollowedAsync()
        {
            try
            {
                // Retrieve the JWT token from the Authorization header
                var authorizationHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
                if (authorizationHeader == null)
                {
                    throw new ArgumentNullException(nameof(authorizationHeader));
                }
                var user = await _authorizationService.GetAuthenticatedUserAsync(authorizationHeader);

                if (user.Id < 1)
                {
                    _logger.LogInformation("User is not logged in.");
                    return StatusCode(401, "User is not logged in.");
                }
                _currentUserId = user.Id;

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
        [HttpPost("follow_user")]
        public async Task<IActionResult> Create([FromBody] int userId)
        {
            try
            {
                // Retrieve the JWT token from the Authorization header
                var authorizationHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
                if (authorizationHeader == null)
                {
                    throw new ArgumentNullException(nameof(authorizationHeader));
                }
                var user = await _authorizationService.GetAuthenticatedUserAsync(authorizationHeader);

                if (user.Id < 1)
                {
                    _logger.LogInformation("User is not logged in.");
                    return Json(new FollowResult { Success = false, ErrorMessage = "User is not logged in." });
                }

                _currentUserId = user.Id;

                if (ModelState.IsValid  && userId > 0)
                {
                    _ = _context.Add(new Follows() { FollowedUserId = userId, FollowerUserId = (int)_currentUserId });
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Follow created: FollowerUserId = {_currentUserId}, FollowedUserId = {userId}");
                    return Json(new FollowResult { Success = true });
                }
                _logger.LogError("ModelState is invalid");
                return Json(new FollowResult { Success = false, ErrorMessage = "An error occurred while attempting to follow. Invalid input data." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating follow");
                return Json(new FollowResult { Success = false, ErrorMessage = "An error occurred while attempting to follow." });
            }
        }

        private bool FollowsExists(int id)
        {
            return (_context.Follows?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
