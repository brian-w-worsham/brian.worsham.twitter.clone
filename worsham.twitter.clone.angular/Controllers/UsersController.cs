using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using worsham.twitter.clone.angular.Models.EntityModels;
using worsham.twitter.clone.angular.Services;

namespace worsham.twitter.clone.angular.Controllers
{
    [EnableCors("AllowOrigin")]
    // [EnableCors("AllowAll")]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : TwitterController
    {
        private readonly TwitterCloneContext _context;
        private readonly IAuthenticationService _authenticationService;

        public UsersController(
            TwitterCloneContext context,
            IAuthenticationService authenticationService,
            ILogger<UsersController> logger,
            IAuthorizationService authorizationService
        ) : base(logger, authorizationService)
        {
            _context = context;
            _authenticationService = authenticationService;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Users>>> GetUsers()
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Users>> GetUsers(int id)
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            var users = await _context.Users.FindAsync(id);

            if (users == null)
            {
                return NotFound();
            }

            return users;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsers(int id, Users users)
        {
            if (id != users.Id)
            {
                return BadRequest();
            }

            _context.Entry(users).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsersExists(id))
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

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPost]
        //public async Task<ActionResult<Users>> PostUsers(Users users)
        //{
        //    if (_context.Users == null)
        //    {
        //        return Problem("Entity set 'TwitterCloneContext.Users'  is null.");
        //    }
        //    _context.Users.Add(users);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetUsers", new { id = users.Id }, users);
        //}

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsers(int id)
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            var users = await _context.Users.FindAsync(id);
            if (users == null)
            {
                return NotFound();
            }

            _context.Users.Remove(users);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UsersExists(int id)
        {
            return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        /// <summary>
        /// Handles the HTTP POST request for creating a new user account.
        /// </summary>
        /// <param name="user">
        /// The <see cref="Users"/> object containing the user's registration data.
        /// </param>
        /// <returns>
        /// A JSON object indicating the success or failure of the account creation attempt. If
        /// successful, the response contains a <c>success</c> value set to <c>true</c>. If
        /// unsuccessful, the response contains a <c>success</c> value set to <c>false</c> and an
        /// <c>errorMessage</c> describing the reason for failure.
        /// </returns>
        /// <remarks>
        /// This method handles the registration of a new user account by validating the input data,
        /// checking if the username is already taken, and registering the user using the provided
        /// <see cref="IAuthenticationService"/>. If the registration is successful, the method logs
        /// the event and returns a success JSON response. If a <see cref="DbUpdateException"/>
        /// occurs, the method checks if it's a unique constraint violation and returns an
        /// appropriate JSON response. For any other exceptions, the method logs the error and
        /// returns an error JSON response.
        /// </remarks>
        // [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] Users user
        )
        {
            if (ModelState.IsValid)
            {
                try
                {
                    bool isUsernameTaken = await _authenticationService.IsUsernameTaken(
                        user.UserName
                    );

                    if (isUsernameTaken)
                    {
                        return Json(
                            new
                            {
                                success = false,
                                errorMessage = "This username is already taken."
                            }
                        );
                    }

                    await _authenticationService.RegisterUser(user, user.Password);

                    base._logger.LogInformation(
                        "User registered successfully: {Username}",
                        user.UserName
                    );

                    return Json(new { success = true });
                }
                catch (DbUpdateException ex)
                {
                    if (IsUniqueConstraintViolation(ex))
                    {
                        base._logger.LogWarning(
                            "Attempt to register with a non-unique username: {Username}",
                            user.UserName
                        );
                        return Json(
                            new
                            {
                                success = false,
                                errorMessage = "This username is already taken."
                            }
                        );
                    }
                    else
                    {
                        base._logger.LogError(ex, "An error occurred while registering a user.");
                        return Json(
                            new
                            {
                                success = false,
                                errorMessage = "An error occurred while registering. Please try again later."
                            }
                        );
                    }
                }
            }
            return Json(new { success = false, errorMessage = "Invalid input data." });
        }

        /// <summary>
        /// Checks whether the provided <see cref="DbUpdateException"/> indicates a unique constraint violation.
        /// </summary>
        /// <param name="ex">The <see cref="DbUpdateException"/> to check.</param>
        /// <returns>
        /// Returns true if the exception message or inner exception message indicates a unique constraint violation for the "IX_Users_UserName" index; otherwise, returns false.
        /// </returns>
        /// <remarks>
        /// This method checks the provided <see cref="DbUpdateException"/> and its inner exception, if present, for messages indicating a unique constraint violation.
        /// It returns true if either the exception message or the inner exception message contains the unique constraint index name "IX_Users_UserName"; otherwise, it returns false.
        /// This is useful for identifying cases where a duplicate user name has been attempted to be inserted into the database, violating the unique constraint.
        /// </remarks>
        private bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            return ex.InnerException?.Message.Contains("IX_Users_UserName") == true
                || ex.Message.Contains("IX_Users_UserName");
        }
    }
}
