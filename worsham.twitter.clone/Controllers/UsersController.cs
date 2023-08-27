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
using worsham.twitter.clone.Services;

namespace worsham.twitter.clone.Controllers
{
    public class UsersController : Controller
    {
        private readonly TwitterCloneContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<UsersController> _logger;
        private int? _currentUserId;

        public UsersController(TwitterCloneContext context, IAuthenticationService authenticationService, ILogger<UsersController> logger)
        {
            _context = context;
            _authenticationService = authenticationService;
            _logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _currentUserId = HttpContext.Session.GetInt32("UserId");
            base.OnActionExecuting(context);
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

        [HttpGet]
        public async Task<IActionResult> Profile(int? id)
        {
            /* 
             * Create and return a Profile View along with the data needed to build out the Profile page for the current user.
             * The current user's id is stored in the _currentUserId field.
             * We can use _currentUserId to get the current user's data from the _context.Users entity model
             * The requirements for the profile page are for it to display the user's name, bio, profile picture, number of followers, list of tweets, list of liked tweets, and a list of ReTweets.
             * Most of the required data will be available from the _context.Users object, but we may have to retrieve some data from other tables and return it to the View as ViewData or as a custom model.
             */
            try
            {
                int? userId;

                if (id.HasValue && id.Value != _currentUserId)
                {
                    userId = id.Value; // Use the provided userId if available
                    ViewData["UserIsViewingOwnProfile"] = false;
                    // Check if the current user is following the user whose profile is being viewed
                    ViewData["CurrentUserIsFollowing"] = _context.Follows.Any(f => f.FollowerUserId == _currentUserId && f.FollowedUserId == userId);
                }
                else
                {
                    userId = _currentUserId; // Use the default _currentUserId if no userId is provided
                    ViewData["UserIsViewingOwnProfile"] = true;
                }

                // Get the current user's data
                Users? currentUser = _context?.Users?.Include(u => u.Tweets).Include(u => u.Likes).Include(u => u.ReTweets).FirstOrDefault(u => u.Id == userId);

                if (currentUser == null)
                {
                    // Handle the case where the current user is not found
                    // Todo: display a notification to the user that the requested user was not found
                    return NotFound();
                }

                // Create a custom profile model to hold the necessary data
                UserProfileModel userProfile = new UserProfileModel
                {
                    UserName = currentUser.UserName,
                    Bio = currentUser.Bio,
                    ProfilePictureUrl = currentUser.ProfilePictureUrl,
                    FollowersCount = _context.Follows.Count(f => f.FollowedUserId == currentUser.Id),
                    FollowingCount = _context.Follows.Count(f => f.FollowerUserId == currentUser.Id),
                    Tweets = currentUser.Tweets.OrderByDescending(t => t.CreationDateTime).ToList(),
                    RetweetedTweets = _context.ReTweets.Where(r => r.RetweeterId == _currentUserId).OrderByDescending(r => r.OriginalTweet.CreationDateTime).Select(r => new RetweetedTweetInfo
                    {
                        OriginalTweet = r.OriginalTweet,
                        OriginalUserName = r.OriginalTweet.Tweeter.UserName,
                        OriginalTweetCreationDateTime = r.OriginalTweet.CreationDateTime,
                        OriginalTweetContent = r.OriginalTweet.Content
                    }).ToList(),
                    /* The LikedTweets property should be a list of the tweets that the current user has liked and it should include the UserName of the user that originally tweeted the liked tweet.
                     It should also include the CreationDateTime and Content of the liked tweet.
                     */
                    LikedTweetInfos = _context.Likes.Where(l => l.UserThatLikedTweetId == _currentUserId).OrderByDescending(l => l.LikedTweet.CreationDateTime).Select(l => new LikedTweetInfo
                    {
                        LikedTweet = l.LikedTweet,
                        OriginalUserName = l.LikedTweet.Tweeter.UserName,
                        OriginalTweetCreationDateTime = l.LikedTweet.CreationDateTime,
                        OriginalTweetContent = l.LikedTweet.Content
                    }).ToList()
                };
                // from userProfile.Retweets retrieve the first tweet that the current user has ReTweeted and then log the UserName of the user that originally tweeted the tweet

                return View(userProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile data");
                return RedirectToAction("Error", "Home");
            }
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

        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            // Get the authenticated user's ID
            int? userId = _currentUserId;

            // Create a directory for the user's profile pictures
            string profilePicturesPath = Path.Combine("wwwroot", "uploads", "profile_pictures", userId?.ToString());
            Directory.CreateDirectory(profilePicturesPath);

            // Generate a unique filename for the uploaded file
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(profilePicturesPath, uniqueFileName);

            // Save the file to the server
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Save the file path (relative to wwwroot) to the user's profilePictureUrl property in the database
            // UpdateUserProfilePictureUrl(userId, $"/uploads/profile_pictures/{userId}/{uniqueFileName}");

            // Redirect or return a response indicating success
            return RedirectToAction("Profile", "User", new { id = userId });
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
