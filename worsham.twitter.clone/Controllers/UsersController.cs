using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Authentication;
using worsham.twitter.clone.Models;
using worsham.twitter.clone.Models.EntityModels;
using worsham.twitter.clone.Services;

namespace worsham.twitter.clone.Controllers
{
    public class UsersController : TwitterController
    {
        private readonly TwitterCloneContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthenticationService _authenticationService;
        private int? _currentUserId;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UsersController(TwitterCloneContext context, IHttpContextAccessor httpContextAccessor, IAuthenticationService authenticationService, ILogger<UsersController> logger, IWebHostEnvironment webHostEnvironment, IAuthorizationService authorizationService) : base(logger, authorizationService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _authenticationService = authenticationService;
            _webHostEnvironment = webHostEnvironment;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _currentUserId = HttpContext.Session.GetInt32("UserId");
            base.OnActionExecuting(context);
        }

        /// <summary>
        /// Displays a list of users, if authorized as an admin.
        /// </summary>
        /// <returns>
        /// If the user is authorized as an admin, returns a view displaying the list of users from the <see cref="TwitterCloneContext.Users"/>.
        /// If the user is not authorized as an admin, redirects the user to the appropriate page using the <see cref="RedirectIfNotAdmin"/> method.
        /// If the <see cref="TwitterCloneContext.Users"/> is null, returns a "Problem" response with a specific error message.
        /// </returns>
        /// <remarks>
        /// This method displays the list of users if the user is authorized as an admin.
        /// It first checks whether the user is authorized as an admin by calling the <see cref="RedirectIfNotAdmin"/> method.
        /// If the user is authorized, the method checks whether the <see cref="TwitterCloneContext.Users"/> is null.
        /// If it is null, a "Problem" response is returned with a specific error message.
        /// Otherwise, the method retrieves the list of users from the <see cref="TwitterCloneContext.Users"/> using the <see cref="DbContext.ToListAsync"/> method.
        /// The list of users is then passed to the view and returned as a result.
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

            return _context.Users != null ? View(await _context.Users.ToListAsync()) : Problem("Entity set 'TwitterCloneContext.Users'  is null.");
        }

        /// <summary>
        /// Displays details of a user, if authorized as an admin.
        /// </summary>
        /// <param name="id">The ID of the user to display details for.</param>
        /// <returns>
        /// If the user is authorized as an admin and a valid user ID is provided, returns a view displaying the details of the user from the <see cref="TwitterCloneContext.Users"/>.
        /// If the user is not authorized as an admin, redirects the user to the appropriate page using the <see cref="RedirectIfNotAdmin"/> method.
        /// If the provided user ID is null or the <see cref="TwitterCloneContext.Users"/> is null, returns a "NotFound" response.
        /// </returns>
        /// <remarks>
        /// This method displays the details of a user if the user is authorized as an admin and a valid user ID is provided.
        /// It first checks whether the user is authorized as an admin by calling the <see cref="RedirectIfNotAdmin"/> method.
        /// If the user is authorized, the method checks whether the provided user ID is null or the <see cref="TwitterCloneContext.Users"/> is null.
        /// If either of these conditions is true, a "NotFound" response is returned.
        /// Otherwise, the method retrieves the user details from the <see cref="TwitterCloneContext.Users"/> using the provided user ID.
        /// If the user is found, their details are passed to the view and returned as a result.
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
        /// Displays the profile of a user, either the current user's own profile or the profile of
        /// a user they are viewing.
        /// </summary>
        /// <param name="followedUserId">
        /// The ID of the user whose profile is being viewed. If null, the current user's own
        /// profile is displayed.
        /// </param>
        /// <returns>An <see cref="IActionResult"/> representing the view of the user's profile.</returns>
        [HttpGet]
        public IActionResult Profile(int? followedUserId)
        {
            try
            {
                // Check whether the user is logged in. If they are not logged in redirect them to
                // the Home/Index view.
                if (_currentUserId == null)
                {
                    base._logger.LogInformation("User is not logged in. Redirecting to Home/Index.");
                    return RedirectToAction("Index", "Home");
                }

                int? userId = GetUserIdForProfileView(followedUserId);
                UserProfileModel userProfile = GetUserProfile(userId);

                ViewData["TweeterProfilePictureUrls"] = GetProfilePictureUrls(userProfile.Tweets);
                ViewData["RetweeterProfilePictureUrls"] = GetProfilePictureUrls(userProfile.RetweetedTweets.Select(r => r.OriginalTweet));
                ViewData["LikedProfilePictureUrls"] = GetProfilePictureUrls(userProfile.LikedTweetInfos.Select(l => l.LikedTweet));
                ViewData["page"] = "profile";

                ViewData["errorNotification"] = (string.IsNullOrEmpty(TempData["errorNotification"]?.ToString())) ? "" : TempData["errorNotification"]?.ToString();
                return View(userProfile);
            }
            catch (Exception ex)
            {
                base._logger.LogError(ex, "Error retrieving user profile data");
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Determines the user ID for profile view based on whether the user is viewing their own
        /// profile or another user's profile.
        /// </summary>
        /// <param name="followedUserId">The user ID of the profile being viewed, if applicable.</param>
        /// <returns>
        /// If the user is viewing another user's profile, the user ID of the viewed profile. If the
        /// user is viewing their own profile, the current user's ID. Returns null if an error
        /// occurs during the process.
        /// </returns>
        private int? GetUserIdForProfileView(int? followedUserId)
        {
            try
            {
                if (followedUserId.HasValue && followedUserId.Value != _currentUserId)
                {
                    ViewData["UserIsViewingOwnProfile"] = false;

                    bool currentUserIsFollowing = _context.Follows.Any(f => f.FollowerUserId == _currentUserId && f.FollowedUserId == followedUserId);
                    ViewData["CurrentUserIsFollowing"] = currentUserIsFollowing;

                    if (currentUserIsFollowing)
                    {
                        ViewData["FollowId"] = _context.Follows?.FirstOrDefault(f => f.FollowerUserId == _currentUserId && f.FollowedUserId == followedUserId)?.Id;
                    }

                    base._logger.LogInformation("User is viewing another user's profile. Current user ID: {CurrentUserId}, Viewed user ID: {ViewedUserId}", _currentUserId, followedUserId);

                    return followedUserId;
                }
                else
                {
                    ViewData["UserIsViewingOwnProfile"] = true;

                    base._logger.LogInformation("User is viewing their own profile. User ID: {UserId}", _currentUserId);

                    return _currentUserId;
                }
            }
            catch (Exception ex)
            {
                base._logger.LogError(ex, "An error occurred while determining the user's profile view.");
                // Handle the exception if needed
                return null;
            }
        }

        /// <summary>
        /// Retrieves the user profile information for a given user.
        /// </summary>
        /// <param name="userId">The ID of the user for whom to retrieve the profile.</param>
        /// <returns>The user profile model.</returns>
        private UserProfileModel GetUserProfile(int? userId)
        {
            try
            {
                Users? currentUser = _context?.Users?.Include(u => u.Tweets).Include(u => u.Likes).Include(u => u.ReTweets).FirstOrDefault(u => u.Id == userId);

                if (currentUser == null)
                {
                    base._logger.LogError("User profile not found for user with ID: {UserId}", userId);
                    TempData["errorNotification"] = "An error occurred. The requested user profile was not found.";
                    throw new Exception("User not found.");
                }

                return new UserProfileModel
                {
                    UserId = currentUser.Id,
                    UserName = currentUser.UserName,
                    Bio = currentUser.Bio,
                    ProfilePictureUrl = currentUser.ProfilePictureUrl ?? "\\default\\1.jpg",
                    FollowersCount = _context.Follows.Count(f => f.FollowedUserId == currentUser.Id),
                    FollowingCount = _context.Follows.Count(f => f.FollowerUserId == currentUser.Id),
                    Tweets = currentUser.Tweets.OrderByDescending(t => t.CreationDateTime).ToList(),
                    RetweetedTweets = GetRetweetedTweets(userId),
                    LikedTweetInfos = GetLikedTweets(userId)
                };
            }
            catch (Exception ex)
            {
                base._logger.LogError(ex, "An error occurred while retrieving user profile for user with ID: {UserId}", userId);
                throw; // Re-throw the exception for proper handling at a higher level
            }
        }

        /// <summary>
        /// Retrieves the retweeted tweets for a given user.
        /// </summary>
        /// <param name="userId">The ID of the user for whom to retrieve retweeted tweets.</param>
        /// <returns>A list of retweeted tweet information.</returns>
        private List<RetweetedTweetInfo> GetRetweetedTweets(int? userId)
        {
            return _context.ReTweets
                .Where(r => r.RetweeterId == userId)
                .OrderByDescending(r => r.OriginalTweet.CreationDateTime)
                .Select(r => new RetweetedTweetInfo
                {
                    OriginalTweet = r.OriginalTweet,
                    OriginalUserName = r.OriginalTweet.Tweeter.UserName,
                    OriginalTweetCreationDateTime = r.OriginalTweet.CreationDateTime,
                    OriginalTweetContent = r.OriginalTweet.Content
                }).ToList();
        }

        /// <summary>
        /// Retrieves the liked tweets for a given user.
        /// </summary>
        /// <param name="userId">The ID of the user for whom to retrieve liked tweets.</param>
        /// <returns>A list of liked tweet information.</returns>
        private List<LikedTweetInfo> GetLikedTweets(int? userId)
        {
            return _context.Likes
                .Where(l => l.UserThatLikedTweetId == userId)
                .OrderByDescending(l => l.LikedTweet.CreationDateTime)
                .Select(l => new LikedTweetInfo
                {
                    LikedTweet = l.LikedTweet,
                    OriginalUserName = l.LikedTweet.Tweeter.UserName,
                    OriginalTweetCreationDateTime = l.LikedTweet.CreationDateTime,
                    OriginalTweetContent = l.LikedTweet.Content
                }).ToList();
        }

        /// <summary>
        /// Retrieves the profile picture URLs associated with a collection of tweets.
        /// </summary>
        /// <param name="tweets">
        /// The collection of tweets for which to retrieve profile picture URLs.
        /// </param>
        /// <returns>
        /// A dictionary containing tweet IDs as keys and their associated profile picture URLs as values.
        /// </returns>
        private Dictionary<int, string> GetProfilePictureUrls(IEnumerable<Tweets> tweets)
        {
            Dictionary<int, string> profilePictureUrls = new Dictionary<int, string>();

            try
            {
                foreach (var tweet in tweets)
                {
                    if (!profilePictureUrls.ContainsKey(tweet.Id))
                    {
                        var user = _context.Users.FirstOrDefault(u => u.Id == tweet.TweeterId);
                        string profilePictureUrl = user?.ProfilePictureUrl ?? "\\default\\1.jpg";

                        profilePictureUrls.Add(tweet.Id, profilePictureUrl);

                        base._logger.LogInformation("Profile picture URL retrieved for tweet ID {TweetId}: {ProfilePictureUrl}", tweet.Id, profilePictureUrl);
                    }
                }
            }
            catch (Exception ex)
            {
                base._logger.LogError(ex, "An error occurred while retrieving profile picture URLs for tweets.");
            }

            return profilePictureUrls;
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
                        return Json(new { success = false, errorMessage = "This username is already taken." });
                    }

                    await _authenticationService.RegisterUser(user, user.Password);

                    base._logger.LogInformation("User registered successfully: {Username}", user.UserName);

                    return Json(new { success = true });
                }
                catch (DbUpdateException ex)
                {
                    if (IsUniqueConstraintViolation(ex))
                    {
                        base._logger.LogWarning("Attempt to register with a non-unique username: {Username}", user.UserName);
                        return Json(new { success = false, errorMessage = "This username is already taken." });
                    }
                    else
                    {
                        base._logger.LogError(ex, "An error occurred while registering a user.");
                        return Json(new { success = false, errorMessage = "An error occurred while registering. Please try again later." });
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
            return ex.InnerException?.Message.Contains("IX_Users_UserName") == true || ex.Message.Contains("IX_Users_UserName");
        }

        /// <summary>
        /// Displays a form for editing user details, if authorized as an admin.
        /// </summary>
        /// <param name="id">The ID of the user to edit.</param>
        /// <returns>
        /// If the user is authorized as an admin and a valid user ID is provided, returns a view displaying a form for editing user details from the <see cref="TwitterCloneContext.Users"/>.
        /// If the user is not authorized as an admin, redirects the user to the appropriate page using the <see cref="RedirectIfNotAdmin"/> method.
        /// If the provided user ID is null or the <see cref="TwitterCloneContext.Users"/> is null, returns a "NotFound" response.
        /// If the user is found in the <see cref="TwitterCloneContext.Users"/>, their details are passed to the view and returned as a result.
        /// </returns>
        /// <remarks>
        /// This method displays a form for editing user details if the user is authorized as an admin and a valid user ID is provided.
        /// It first checks whether the user is authorized as an admin by calling the <see cref="RedirectIfNotAdmin"/> method.
        /// If the user is authorized, the method checks whether the provided user ID is null or the <see cref="TwitterCloneContext.Users"/> is null.
        /// If either of these conditions is true, a "NotFound" response is returned.
        /// Otherwise, the method retrieves the user details from the <see cref="TwitterCloneContext.Users"/> using the provided user ID.
        /// If the user is found, their details are passed to the view for editing and returned as a result.
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
                    string fileName = null;
                    if (userProfile.FormFile != null)
                    {
                        string wwwRootPath = _webHostEnvironment.WebRootPath;
                        Guid guid = Guid.NewGuid();
                        fileName = Path.GetFileNameWithoutExtension(userProfile.FormFile.FileName) + guid.ToString();
                        string extension = Path.GetExtension(userProfile.FormFile.FileName);
                        fileName = fileName + extension;
                        string path = Path.Combine(wwwRootPath + "/uploads/profile_pictures/", fileName);

                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await userProfile.FormFile.CopyToAsync(fileStream);
                        }

                        base._logger.LogInformation("Profile picture uploaded successfully for user with ID: {UserId}", userProfile.UserId);
                    }

                    _context.Update(entity: new Users()
                    {
                        Id = userProfile.UserId,
                        UserName = userProfile.UserName,
                        Bio = userProfile.Bio,
                        ProfilePictureUrl = fileName ?? _context.Users.Where(u => u.Id == userProfile.UserId).Select(u => u.ProfilePictureUrl).FirstOrDefault(),
                        Email = _context.Users.Where(u => u.Id == userProfile.UserId).Select(u => u.Email).FirstOrDefault(),
                        Password = _context.Users.Where(u => u.Id == userProfile.UserId).Select(u => u.Password).FirstOrDefault()
                    });
                    await _context.SaveChangesAsync();
                    // Mark profile picture upload as successful
                    didProfilePictureUploadSucceed = true;
                }
                catch (DbUpdateConcurrencyException)
                {
                    base._logger.LogError("Concurrency exception while updating profile for user with ID: {UserId}", userProfile.UserId);

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
                    base._logger.LogError(ex, "An error occurred while editing the user profile for user with ID: {UserId}", userProfile.UserId);
                }

                if (!didProfilePictureUploadSucceed)
                {
                    TempData["errorNotification"] = "An error occurred. The user's profile picture upload failed.";
                    ViewData["didProfilePictureUploadSucceed"] = false;
                }
                else
                {
                    ViewData["didProfilePictureUploadSucceed"] = true;
                }

                base._logger.LogInformation("Profile picture upload result: {UploadResult}", didProfilePictureUploadSucceed);

                return RedirectToAction(actionName: "Profile", controllerName: "Users");
            }
            return View(model: userProfile);
        }

        /// <summary>
        /// Handles the HTTP GET request for deleting a user with the specified ID.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>
        /// If the user is authorized to perform the action, returns the view displaying the user details for confirmation.
        /// If the user is not authorized, redirects to the appropriate action based on authorization status.
        /// If the ID is null or no user with the provided ID is found in the database, returns a "Not Found" response.
        /// </returns>
        /// <remarks>
        /// This method handles the HTTP GET request for deleting a user with the specified ID.
        /// It first checks if the user is authorized to perform the deletion. If the user is not authorized,
        /// it redirects them to the appropriate action based on their authorization status.
        /// If the user is authorized, the method attempts to retrieve the user details from the database using the provided ID.
        /// If the ID is null or no user is found with the provided ID, a "Not Found" response is returned.
        /// Otherwise, the method returns a view displaying the user details for confirmation of deletion.
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            var isAuthorized = RedirectIfNotAdmin();
            if (isAuthorized != null)
            {
                return isAuthorized;
            }

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
        /// Handles the HTTP POST request for confirming the deletion of a user.
        /// </summary>
        /// <param name="id">The ID of the user to be deleted.</param>
        /// <returns>
        /// If the user is authorized to perform the action, deletes the user from the database and redirects to the "Index" action.
        /// If the user is not authorized, redirects to the appropriate action based on authorization status.
        /// If the Users entity set is null, returns a "Problem" response indicating the issue.
        /// </returns>
        /// <remarks>
        /// This method handles the HTTP POST request for confirming the deletion of a user with the specified ID.
        /// It first checks if the user is authorized to perform the deletion. If the user is not authorized,
        /// it redirects them to the appropriate action based on their authorization status.
        /// If the user is authorized, the method attempts to delete the user with the provided ID from the database.
        /// If the Users entity set is null, a "Problem" response indicating the issue is returned.
        /// After successful deletion, the method redirects to the "Index" action of the controller.
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

        /// <summary>
        /// Handles the HTTP POST request for user login.
        /// </summary>
        /// <param name="userName">The username entered by the user.</param>
        /// <param name="password">The password entered by the user.</param>
        /// <returns>An <see cref="IActionResult"/> representing the action result.</returns>
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var user = await _authenticationService.AuthenticateUser(loginDto.UserName, loginDto.Password);

                // Authentication successful - Set up the session here
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("UserName", user.UserName);
                HttpContext.Session.SetString("UserRole", user.UserRole);

                base._logger.LogInformation("Successful login for user: {UserName}", user.UserName);
                return Json(new LoginResult { Success = true });
            }
            catch (ArgumentException ex) // Replace AuthenticationException with the actual exception type
            {
                // Log the authentication exception
                base._logger.LogError(ex, ex.Message);
                return Json(new LoginResult { Success = false, ErrorMessage = ex.Message });
            }
            catch (AuthenticationException ex) // Replace AuthenticationException with the actual exception type
            {
                // Log the authentication exception
                base._logger.LogError(ex, ex.Message);
                return Json(new LoginResult { Success = false, ErrorMessage = ex.Message });
            }
            catch (Exception ex)
            {
                // Log other exceptions
                base._logger.LogError(ex, ex.Message);
                return Json(new LoginResult { Success = false, ErrorMessage = "An error occurred during login. Please try again later." });
            }
        }

        /// <summary>
        /// Handles the HTTP POST request for user logout, effectively ending the user's session.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> representing the action result.</returns>
        /// <remarks>
        /// This method clears the user's session, effectively logging them out from the
        /// application. It removes all session data associated with the user, providing a secure
        /// way to end their session. If the logout is successful, the method logs the event and
        /// redirects the user to the home page. If an exception occurs during the logout process,
        /// the method logs the error and redirects to the error page.
        /// </remarks>
        [HttpPost]
        public IActionResult Logout()
        {
            try
            {
                _authenticationService.LogoutUser(_httpContextAccessor.HttpContext);

                // Log successful logout
                base._logger.LogInformation("User logged out successfully.");

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Log any errors that occur during logout
                base._logger.LogError(ex, "An error occurred during logout.");

                // Handle the error appropriately, e.g., show an error page
                return RedirectToAction("Error", "Home");
            }
        }
    }
}