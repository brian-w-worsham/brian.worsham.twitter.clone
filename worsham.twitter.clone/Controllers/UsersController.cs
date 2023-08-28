using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UsersController(TwitterCloneContext context, IAuthenticationService authenticationService, ILogger<UsersController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _authenticationService = authenticationService;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
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

        /// <summary>
        /// Retrieves and displays a user's profile data.
        /// </summary>
        /// <param name="followedUserId">The optional ID of the user whose profile is being viewed.</param>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> representing the action result. If successful,
        /// displays the user's profile using the "UserProfile" view. If the user does not exist,
        /// returns a "NotFound" response. If an error occurs during the operation, redirects to the
        /// "Error" action of the "Home" controller.
        /// </returns>
        /// <remarks>
        /// This action retrieves the profile data of a user. If a specific <paramref
        /// name="followedUserId"/> is provided, the profile of that user is displayed. If no
        /// <paramref name="followedUserId"/> is provided or if the provided ID matches the current
        /// user's ID, the profile of the current user is displayed. The method retrieves user data
        /// including tweets, likes, retweets, followers count, and following count. If the user
        /// does not exist, a "NotFound" response is returned. If an exception occurs during the
        /// retrieval or display of data, an error log is generated, and the user is redirected to
        /// the "Error" action of the "Home" controller.
        /// </remarks>
        [HttpGet]
        public IActionResult Profile(int? followedUserId)
        {
            try
            {
                int? userId;

                if (followedUserId.HasValue && followedUserId.Value != _currentUserId)
                {
                    userId = followedUserId.Value; // Use the provided userId if available
                    ViewData["UserIsViewingOwnProfile"] = false;
                    // Check if the current user is following the user whose profile is being viewed
                    bool currentUserIsFollowing = _context.Follows.Any(f => f.FollowerUserId == _currentUserId && f.FollowedUserId == userId);
                    ViewData["CurrentUserIsFollowing"] = currentUserIsFollowing;
                    if (currentUserIsFollowing)
                    {
                        ViewData["FollowId"] = _context.Follows?.FirstOrDefault(f => f.FollowerUserId == _currentUserId && f.FollowedUserId == userId)?.Id;
                    }
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
                    // Todo: display a notification to the user that the requested user was not found
                    return NotFound();
                }

                // Create a custom profile model to hold the necessary data
                UserProfileModel userProfile = new UserProfileModel
                {
                    UserId = currentUser.Id,
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
                    LikedTweetInfos = _context.Likes.Where(l => l.UserThatLikedTweetId == _currentUserId).OrderByDescending(l => l.LikedTweet.CreationDateTime).Select(l => new LikedTweetInfo
                    {
                        LikedTweet = l.LikedTweet,
                        OriginalUserName = l.LikedTweet.Tweeter.UserName,
                        OriginalTweetCreationDateTime = l.LikedTweet.CreationDateTime,
                        OriginalTweetContent = l.LikedTweet.Content
                    }).ToList()
                };

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

        // POST: Users/Create To protect from overposting attacks, enable the specific properties
        // you want to bind to. For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
            // Check if the exception message or inner exception message indicates a unique
            // constraint violation
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

        /// <summary>
        /// Handles the HTTP POST request to edit a user's profile.
        /// </summary>
        /// <param name="UserId">The ID of the user to be edited.</param>
        /// <param name="userProfile">
        /// The model containing user profile information and the uploaded profile picture.
        /// </param>
        /// <returns>An <see cref="IActionResult"/> representing the action result.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int UserId, [Bind("UserId,UserName,Bio,FormFile")] UserProfileModel userProfile)
        {
            if (UserId != userProfile.UserId)
            {
                return NotFound();
            }
            bool didProfilePictureUploadSucceed = false;
            if (ModelState.IsValid)
            {
                try
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    Guid guid = Guid.NewGuid();
                    string fileName = Path.GetFileNameWithoutExtension(userProfile.FormFile.FileName) + guid.ToString();
                    string extension = Path.GetExtension(userProfile.FormFile.FileName);
                    fileName = fileName + extension;
                    string path = Path.Combine(wwwRootPath + "/uploads/profile_pictures/", fileName);

                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await userProfile.FormFile.CopyToAsync(fileStream);
                    }

                    _logger.LogInformation("Profile picture uploaded successfully for user with ID: {UserId}", userProfile.UserId);

                    _context.Update(entity: new Users()
                    {
                        Id = userProfile.UserId,
                        UserName = userProfile.UserName,
                        Bio = userProfile.Bio,
                        ProfilePictureUrl = fileName,
                        Email = _context.Users.Where(u => u.Id == userProfile.UserId).Select(u => u.Email).FirstOrDefault(),
                        Password = _context.Users.Where(u => u.Id == userProfile.UserId).Select(u => u.Password).FirstOrDefault()
                    });
                    await _context.SaveChangesAsync();
                    // Mark profile picture upload as successful
                    didProfilePictureUploadSucceed = true;
                }
                catch (DbUpdateConcurrencyException)
                {
                    _logger.LogError("Concurrency exception while updating profile for user with ID: {UserId}", userProfile.UserId);

                    if (!UsersExists(id: UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while editing the user profile for user with ID: {UserId}", userProfile.UserId);
                }

                if (!didProfilePictureUploadSucceed)
                {
                    // todo: display a notification to the user that the profile picture upload failed
                    ViewData["didProfilePictureUploadSucceed"] = false;
                }
                else
                {
                    ViewData["didProfilePictureUploadSucceed"] = true;
                }

                _logger.LogInformation("Profile picture upload result: {UploadResult}", didProfilePictureUploadSucceed);

                return RedirectToAction(actionName: "Profile", controllerName: "Users");
            }
            return View(model: userProfile);
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
                // Authentication successful Set up the session here
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