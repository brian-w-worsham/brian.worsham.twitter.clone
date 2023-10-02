using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using worsham.twitter.clone.angular.Models;
using worsham.twitter.clone.angular.Models.EntityModels;
using worsham.twitter.clone.angular.Services;
using IAuthorizationService = worsham.twitter.clone.angular.Services.IAuthorizationService;
using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace worsham.twitter.clone.angular.Controllers
{
    [EnableCors("AllowOrigin")]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : TwitterController
    {
        private readonly TwitterCloneContext _context;
        private readonly IAuthenticationService _authenticationService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UsersController(
            TwitterCloneContext context,
            IAuthenticationService authenticationService,
            ILogger<UsersController> logger,
            IAuthorizationService authorizationService,
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment
        ) : base(logger, authorizationService)
        {
            _context = context;
            _authenticationService = authenticationService;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
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
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] Users user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    bool isUsernameTaken = await _authenticationService.IsUsernameTaken(
                        user.UserName
                    );

                    bool isEmailTaken = await _authenticationService.IsEmailTaken(user.Email);

                    if (isUsernameTaken)
                    {
                        return Json(
                            new
                            {
                                success = false,
                                errorMessage = $"The name, {user.UserName}, is already taken."
                            }
                        );
                    }

                    if (isEmailTaken)
                    {
                        return Json(
                            new
                            {
                                success = false,
                                errorMessage = $"The email address, {user.Email}, is already taken."
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
        /// Handles the HTTP POST request for user login.
        /// </summary>
        /// <param name="userName">The username entered by the user.</param>
        /// <param name="password">The password entered by the user.</param>
        /// <returns>An <see cref="IActionResult"/> representing the action result.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var user = await _authenticationService.AuthenticateUser(
                     loginDto.UserName,
                     loginDto.Password
                 );

                // Authentication successful - Generate JWT token
                // retrieve SecretKeyForJwtToken from secrets.json
                var secretKey = _configuration["SecretKeyForJwtToken"];
                var key = Encoding.ASCII.GetBytes(secretKey);

                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.UserRole),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature
                    )
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                // Authentication successful - Set up the session here
                // HttpContext.Session.SetInt32("UserId", user.Id);
                // HttpContext.Session.SetString("UserName", user.UserName);
                // HttpContext.Session.SetString("UserRole", user.UserRole);

                base._logger.LogInformation("Successful login for user: {UserName}", user.UserName);
                // Send JWT token back to front end
                return Json(new TwitterApiActionResult { Success = true, Token = tokenString });
            }
            catch (ArgumentException ex) // Replace AuthenticationException with the actual exception type
            {
                // Log the authentication exception
                base._logger.LogError(ex, ex.Message);
                return Json(new TwitterApiActionResult { Success = false, ErrorMessage = ex.Message });
            }
            catch (AuthenticationException ex) // Replace AuthenticationException with the actual exception type
            {
                // Log the authentication exception
                base._logger.LogError(ex, ex.Message);
                return Json(new TwitterApiActionResult { Success = false, ErrorMessage = ex.Message });
            }
            catch (Exception ex)
            {
                // Log other exceptions
                base._logger.LogError(ex, ex.Message);
                return Json(
                    new TwitterApiActionResult
                    {
                        Success = false,
                        ErrorMessage = "An error occurred during login. Please try again later."
                    }
                );
            }
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
        [HttpGet("get_profile/{followedUserId?}")]
        public async Task<ActionResult<UserProfileModel>> GetProfileAsync(int? followedUserId)
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

                // Check whether the user is logged in. If they are not logged in redirect them to
                // the Home/Index view.
                if (user.Id < 1)
                {
                    _logger.LogInformation("User is not logged in. Redirecting to Home/Index.");
                    return Json(new TwitterApiActionResult { Success = false, ErrorMessage = "User is not logged in." });
                }

                _currentUserId = user.Id;

                FollowContext followContext = GetUserIdForProfileView(followedUserId);
                UserProfileModel userProfile = await GetUserProfile(followContext.followedUserId);
                userProfile.CurrentUserIsFollowing = followContext.CurrentUserIsFollowing;
                userProfile.FollowId = followContext.FollowId;

                if (followedUserId.HasValue && followedUserId.Value != _currentUserId)
                {
                    userProfile.UserIsViewingOwnProfile = false;
                }
                else
                {
                    userProfile.UserIsViewingOwnProfile = true;
                }

                userProfile.TweeterProfilePictureUrls = GetProfilePictureUrls(tweets: userProfile.Tweets);
                var reTweets = await CreateTweetModels(combinedAndSortedTweets: userProfile.RetweetedTweets.Select(r => r.OriginalTweet).ToList());
                userProfile.RetweeterProfilePictureUrls = GetProfilePictureUrls(tweets: reTweets);
                var likedTweets = await CreateTweetModels(userProfile.LikedTweetInfos.Select(l => l.LikedTweet).ToList());
                userProfile.LikedProfilePictureUrls = GetProfilePictureUrls(likedTweets);
                userProfile.ErrorNotification = (string.IsNullOrEmpty(TempData["errorNotification"]?.ToString())) ? "" : TempData["errorNotification"]?.ToString();

                JsonSerializerOptions? options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.Preserve,
                    IncludeFields = true,
                    WriteIndented = true,
                    MaxDepth = 128
                };
                var jsonString = JsonSerializer.Serialize<UserProfileModel>(userProfile, options);

                return Content(jsonString, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile data");
                return Json(new TwitterApiActionResult { Success = false, ErrorMessage = "Error retrieving user profile data" });
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
        private FollowContext GetUserIdForProfileView(int? followedUserId)
        {
            try
            {
                bool currentUserIsFollowing = false;
                int? followId = 0;
                if (followedUserId.HasValue && followedUserId.Value != _currentUserId)
                {
                    currentUserIsFollowing = _context.Follows.Any(f => f.FollowerUserId == _currentUserId && f.FollowedUserId == followedUserId);

                    if (currentUserIsFollowing)
                    {
                        followId = _context.Follows?.FirstOrDefault(f => f.FollowerUserId == _currentUserId && f.FollowedUserId == followedUserId)?.Id;
                    }

                    _logger.LogInformation("User is viewing another user's profile. Current user ID: {CurrentUserId}, Viewed user ID: {ViewedUserId}", _currentUserId, followedUserId);

                    return new FollowContext()
                    {
                        FollowId = followId,
                        CurrentUserIsFollowing = currentUserIsFollowing,
                        followedUserId = followedUserId
                    };
                }
                else
                {
                    _logger.LogInformation("User is viewing their own profile. User ID: {UserId}", _currentUserId);

                    return new FollowContext()
                    {
                        FollowId = followId,
                        CurrentUserIsFollowing = currentUserIsFollowing,
                        followedUserId = _currentUserId
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while determining the user's profile view.");
                // Handle the exception if needed
                return null;
            }
        }

        /// <summary>
        /// Retrieves the user profile information for a given user.
        /// </summary>
        /// <param name="userId">The ID of the user for whom to retrieve the profile.</param>
        /// <returns>The user profile model.</returns>
        private async Task<UserProfileModel> GetUserProfile(int? userId)
        {
            try
            {
                Users? user = _context?.Users?.Include(u => u.Tweets).Include(u => u.Likes).Include(u => u.ReTweets).FirstOrDefault(u => u.Id == userId);

                if (user == null)
                {
                    _logger.LogError("User profile not found for user with ID: {UserId}", userId);
                    TempData["errorNotification"] = "An error occurred. The requested user profile was not found.";
                    throw new Exception("User not found.");
                }

                var tweets = await CreateTweetModels(user.Tweets.OrderByDescending(t => t.CreationDateTime).ToList());

                return new UserProfileModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Bio = user.Bio,
                    ProfilePictureUrl = user.ProfilePictureUrl ?? "\\default\\1.jpg",
                    FollowersCount = _context.Follows.Count(f => f.FollowedUserId == user.Id),
                    FollowingCount = _context.Follows.Count(f => f.FollowerUserId == user.Id),
                    Tweets = tweets,
                    RetweetedTweets = GetRetweetedTweets(userId),
                    LikedTweetInfos = GetLikedTweets(userId)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user profile for user with ID: {UserId}", userId);
                throw; // Re-throw the exception for proper handling at a higher level
            }
        }

        private async Task<List<TweetModel>> CreateTweetModels(List<Tweets> combinedAndSortedTweets)
        {
            var tweetModels = new List<TweetModel>();
            foreach (var tweet in combinedAndSortedTweets)
            {
                _logger.LogInformation("Processing tweet with ID {TweetId}.", tweet.Id);
                var tweeter = await _context.Users.FirstOrDefaultAsync(u => u.Id == tweet.TweeterId);
                tweetModels.Add(new TweetModel()
                {
                    Id = tweet.Id,
                    TimeSincePosted = DateTime.UtcNow - tweet.CreationDateTime,
                    TimeAgo = GetTimeAgo(DateTime.UtcNow - tweet.CreationDateTime),
                    Content = tweet.Content,
                    TweeterUserId = tweet.ReTweets.Any() ? tweet.ReTweets.First().OriginalTweet.TweeterId : tweet.TweeterId,
                    TweeterUserName = tweeter?.UserName,
                    Likes = tweet.Likes.ToList(),
                    Comments = tweet.Comments.ToList(),
                    Retweets = tweet.ReTweets.ToList(),
                    TweeterProfilePictureUrl = tweeter?.ProfilePictureUrl ?? "\\default\\1.jpg"
                });
            }
            return tweetModels;
        }

        private string GetTimeAgo(TimeSpan timeSpan)
        {
            if (timeSpan.TotalSeconds < 1)
            {
                return "just now";
            }
            if (timeSpan.TotalMinutes < 1)
            {
                return $"{timeSpan.Seconds}s";
            }
            if (timeSpan.TotalHours < 1)
            {
                return $"{timeSpan.Minutes}m";
            }
            if (timeSpan.TotalDays < 1)
            {
                return $"{timeSpan.Hours}h";
            }
            if (timeSpan.TotalDays < 30)
            {
                return $"{timeSpan.Days}d";
            }
            if (timeSpan.TotalDays < 365)
            {
                return $"{timeSpan.Days / 30}mo";
            }
            else
            {
                return $"{timeSpan.Days / 365}y";
            }
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
        private List<ProfilePictureUrlModel> GetProfilePictureUrls(List<TweetModel> tweets)
        {
            List<ProfilePictureUrlModel> profilePictureUrlModels = new List<ProfilePictureUrlModel>();

            try
            {
                foreach (var tweet in tweets)
                {
                    // add condition that checks if profilePictureUrlModels contains tweet.Id
                    if (!profilePictureUrlModels.Exists(p => p.TweetId == tweet.Id))
                    {
                        var user = _context.Users.FirstOrDefault(u => u.Id == tweet.TweeterUserId);
                        string profilePictureUrl = user?.ProfilePictureUrl ?? "\\default\\1.jpg";

                        profilePictureUrlModels.Add(new ProfilePictureUrlModel { TweetId = tweet.Id, ProfilePictureUrl = profilePictureUrl });

                        _logger.LogInformation("Profile picture URL retrieved for tweet ID {TweetId}: {ProfilePictureUrl}", tweet.Id, profilePictureUrl);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving profile picture URLs for tweets.");
            }

            return profilePictureUrlModels;
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
                    OriginalTweetId = r.OriginalTweetId,
                    OriginalUserName = r.OriginalTweet.Tweeter.UserName,
                    OriginalProfilePictureUrl = r.OriginalTweet.Tweeter.ProfilePictureUrl ?? "\\default\\1.jpg",
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
                    LikedTweetId = l.LikedTweetId,
                    OriginalProfilePictureUrl = l.LikedTweet.Tweeter.ProfilePictureUrl ?? "\\default\\1.jpg",
                    OriginalUserName = l.LikedTweet.Tweeter.UserName,
                    OriginalTweetCreationDateTime = l.LikedTweet.CreationDateTime,
                    OriginalTweetContent = l.LikedTweet.Content
                }).ToList();
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

        [HttpPut("edit")]
        // public async Task<IActionResult> Edit(int UserId, [Bind("UserId,UserName,Bio,FormFile")] UserProfileModel userProfile)
        public async Task<IActionResult> Edit([FromBody] UserProfileModel userProfile)
        {

            // Retrieve the JWT token from the Authorization header
            var authorizationHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (authorizationHeader == null)
            {
                throw new ArgumentNullException(nameof(authorizationHeader));
            }
            var user = await _authorizationService.GetAuthenticatedUserAsync(authorizationHeader);

            if (user.Id != userProfile.UserId)
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
                        // worsham.twitter.clone.angular\ClientApp\src\assets\images\uploads\profile_pictures
                        string path = Path.Combine("/ClientApp/src/assets/images/uploads/profile_pictures", fileName);

                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await userProfile.FormFile.CopyToAsync(fileStream);
                        }

                        base._logger.LogInformation("Profile picture uploaded successfully for user with ID: {UserId}", userProfile.UserId);
                        // Mark profile picture upload as successful
                        didProfilePictureUploadSucceed = true;
                    }

                    _context.Update(entity: new Users()
                    {
                        Id = userProfile.UserId,
                        UserName = userProfile.UserName,
                        Bio = userProfile.Bio,
                        ProfilePictureUrl = fileName ?? _context.Users.Where(u => u.Id == userProfile.UserId).Select(u => u.ProfilePictureUrl).FirstOrDefault(),
                        Email = _context.Users.Where(u => u.Id == userProfile.UserId).Select(u => u.Email).FirstOrDefault(),
                        Password = _context.Users.Where(u => u.Id == userProfile.UserId).Select(u => u.Password).FirstOrDefault(),
                        UserRole = _context.Users.Where(u => u.Id == userProfile.UserId).Select(u => u.UserRole).FirstOrDefault(),
                    });
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("User profile updated for user with ID: {UserId}", userProfile.UserId);
                }
                catch (DbUpdateConcurrencyException)
                {
                    base._logger.LogError("Concurrency exception while updating profile for user with ID: {UserId}", userProfile.UserId);

                    if (!UsersExists(id: userProfile.UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        return Json(new TwitterApiActionResult { Success = false, ErrorMessage = "Concurrency exception while updating profile for user with ID: " + userProfile.UserId.ToString() });
                    }
                }
                catch (Exception ex)
                {
                    base._logger.LogError(ex, "An error occurred while editing the user profile for user with ID: {UserId}", userProfile.UserId);
                    return Json(new TwitterApiActionResult { Success = false, ErrorMessage = "An error occurred while editing the user profile for user with ID: " + userProfile.UserId.ToString() });
                }

                if (!didProfilePictureUploadSucceed)
                {
                    return Json(new TwitterApiActionResult { Success = false, ErrorMessage = "An error occurred. The user's profile picture upload failed." });
                }
                else
                {
                    ViewData["didProfilePictureUploadSucceed"] = true;
                    return Json(new TwitterApiActionResult { Success = true });
                }

                base._logger.LogInformation("Profile picture upload result: {UploadResult}", didProfilePictureUploadSucceed);

            }
            // return View(model: userProfile);
            return Json(new TwitterApiActionResult { Success = false, ErrorMessage = "Invalid input data." });
        }
    }
}
