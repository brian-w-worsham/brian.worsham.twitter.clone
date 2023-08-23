using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using worsham.twitter.clone.Models;
using worsham.twitter.clone.Services;

namespace worsham.twitter.clone.Controllers
{
    public class UsersController : Controller
    {
        private readonly TwitterCloneContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(TwitterCloneContext context, IAuthenticationService authenticationService, ILogger<UsersController> logger)
        {
            _context = context;
            _authenticationService = authenticationService;
            _logger = logger;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            return _context.Users != null ? View(await _context.Users.ToListAsync()) : Problem("Entity set 'TwitterCloneContext.Users'  is null.");
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var users = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
            if (users == null)
            {
                return NotFound();
            }

            return View(users);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserName,Email,Password,Bio")] Users user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    bool isUsernameTaken = await _authenticationService.IsUsernameTaken(user.UserName);

                    if (isUsernameTaken)
                    {
                        ModelState.AddModelError("UserName", "This username is already taken.");
                        return View(user);
                    }

                    await _authenticationService.RegisterUser(user, user.Password);
                    return RedirectToAction("DisplaySignInModal", "Home");
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "An error occurred while registering a user.");
                    // Check if the exception message indicates a unique constraint violation
                    if (IsUniqueConstraintViolation(ex))
                    {
                        ModelState.AddModelError("UserName", "This username is already taken.");
                        // Log the unique constraint violation
                        _logger.LogWarning("Attempt to register with a non-unique username: {Username}", user.UserName);

                        return View(user);
                    }
                    else
                    {
                        // Handle other exceptions as needed
                        throw;
                    }
                }

            }
            return View(user);
        }

        private bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            // Check if the exception message or inner exception message indicates a unique constraint violation
            return ex.InnerException?.Message.Contains("IX_Users_UserName") == true ||
                   ex.Message.Contains("IX_Users_UserName");
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var users = await _context.Users.FindAsync(id);
            if (users == null)
            {
                return NotFound();
            }
            return View(users);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserName,Email,Password,Bio")] Users users)
        {
            if (id != users.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(users);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsersExists(users.Id))
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
            return View(users);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var users = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (users == null)
            {
                return NotFound();
            }

            return View(users);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'TwitterCloneContext.Users'  is null.");
            }
            var users = await _context.Users.FindAsync(id);
            if (users != null)
            {
                _context.Users.Remove(users);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UsersExists(int id)
        {
            return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string userName, string password)
        {
            var user = await _authenticationService.AuthenticateUser(userName, password);

            if (user != null)
            {
                // Authentication successful
                // Set up the session here
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("UserName", user.UserName);

                return RedirectToAction("Index", "Tweets"); // Redirect to the feed page after login
            }
            else
            {
                ModelState.AddModelError("", "Invalid credentials");
                return View();
            }
        }

        [HttpPost]
        public IActionResult Logout()
        {
            _authenticationService.LogoutUser(_httpContextAccessor.HttpContext);
            return RedirectToAction("Index", "Home"); // Redirect to the home page after logout
        }
    }
}
