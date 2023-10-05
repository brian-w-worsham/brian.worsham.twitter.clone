using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using worsham.twitter.clone.angular.Models;
using worsham.twitter.clone.angular.Models.EntityModels;
using worsham.twitter.clone.angular.Services;
using IAuthorizationService = worsham.twitter.clone.angular.Services.IAuthorizationService;

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

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var authorizationHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
                if (authorizationHeader == null)
                {
                    throw new ArgumentNullException(nameof(authorizationHeader));
                }
                var user = await _authorizationService.GetAuthenticatedUserAsync(authorizationHeader);

                if (user.Id < 1 )
                {
                    return NotFound();
                }

                // Remove the JWT token from the client-side storage
                HttpContext.Response.Cookies.Delete("jwt");

                // Perform any other necessary logout actions here

                return Json(new TwitterApiActionResult { Success = true });
            }
            catch (Exception ex)
            {
                // Log any errors that occur during logout
                base._logger.LogError(ex, ex.Message);
                return Json(
                    new TwitterApiActionResult
                    {
                        Success = false,
                        ErrorMessage = "An error occurred during logout. Please try again later."
                    }
                );
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var user = await _authenticationService.AuthenticateUser(
                     loginDto.UserName,
                     loginDto.Password
                 );

                // Authentication successful - Generate JWT token retrieve SecretKeyForJwtToken from secrets.json
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
                    ProfilePictureUrl = user.ProfilePictureUrl ?? "assets\\images\\uploads\\profile_pictures\\default\\1.jpg",
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
                    TweeterProfilePictureUrl = tweeter?.ProfilePictureUrl ?? "assets\\images\\uploads\\profile_pictures\\default\\1.jpg"
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
                        string profilePictureUrl = user?.ProfilePictureUrl ?? "assets\\images\\uploads\\profile_pictures\\default\\1.jpg";

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
                    OriginalProfilePictureUrl = r.OriginalTweet.Tweeter.ProfilePictureUrl ?? "assets\\images\\uploads\\profile_pictures\\default\\1.jpg",
                    OriginalTweetCreationDateTime = r.OriginalTweet.CreationDateTime,
                    OriginalTweetContent = r.OriginalTweet.Content
                }).ToList();
        }

        private List<LikedTweetInfo> GetLikedTweets(int? userId)
        {
            return _context.Likes
                .Where(l => l.UserThatLikedTweetId == userId)
                .OrderByDescending(l => l.LikedTweet.CreationDateTime)
                .Select(l => new LikedTweetInfo
                {
                    LikedTweet = l.LikedTweet,
                    LikedTweetId = l.LikedTweetId,
                    OriginalProfilePictureUrl = l.LikedTweet.Tweeter.ProfilePictureUrl ?? "assets\\images\\uploads\\profile_pictures\\default\\1.jpg",
                    OriginalUserName = l.LikedTweet.Tweeter.UserName,
                    OriginalTweetCreationDateTime = l.LikedTweet.CreationDateTime,
                    OriginalTweetContent = l.LikedTweet.Content
                }).ToList();
        }

        private bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            return ex.InnerException?.Message.Contains("IX_Users_UserName") == true
                || ex.Message.Contains("IX_Users_UserName");
        }

        [HttpPost("upload"), DisableRequestSizeLimit]
        public async Task<IActionResult> Upload()
        {
            // Retrieve the JWT token from the Authorization header
            var authorizationHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (authorizationHeader == null)
            {
                throw new ArgumentNullException(nameof(authorizationHeader));
            }
            var user = await _authorizationService.GetAuthenticatedUserAsync(authorizationHeader);

            if (user == null)
            {
                return NotFound();
            }

            try
            {
                var formCollection = await Request.ReadFormAsync();
                var file = formCollection.Files.First();
                // worsham.twitter.clone.angular\ClientApp\src\assets\images\uploads\profile_pictures
                var folderName = Path.Combine("ClientApp", "src");
                // add assets to the folderName
                folderName = Path.Combine(folderName, "assets");
                // add images to the folderName
                folderName = Path.Combine(folderName, "images");
                // add uploads to the folderName
                folderName = Path.Combine(folderName, "uploads");
                // add profile_pictures to the folderName
                folderName = Path.Combine(folderName, "profile_pictures");
                string relativeFolderName = Path.Combine("assets", "images", "uploads", "profile_pictures");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                if (file.Length > 0)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    var fullPath = Path.Combine(pathToSave, fileName);
                    var dbPath = Path.Combine(relativeFolderName, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    return Ok(new { dbPath });
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        [HttpPost("edit")]
        public async Task<IActionResult> Edit([FromBody] EditProfileModel userProfile)
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

            if (ModelState.IsValid)
            {
                try
                {
                    // Detach the existing entity before updating it
                    var existingUser = _context.Users.Find(userProfile.UserId);
                    if (existingUser != null)
                    {
                        _context.Entry(existingUser).State = EntityState.Detached;
                    }

                    _context.Update(entity: new Users()
                    {
                        Id = userProfile.UserId,
                        UserName = userProfile.UserName,
                        Bio = userProfile.Bio,
                        ProfilePictureUrl = userProfile.ProfilePictureUrl ?? _context.Users.Where(u => u.Id == userProfile.UserId).Select(u => u.ProfilePictureUrl).FirstOrDefault(),
                        Email = _context.Users.Where(u => u.Id == userProfile.UserId).Select(u => u.Email).FirstOrDefault(),
                        Password = _context.Users.Where(u => u.Id == userProfile.UserId).Select(u => u.Password).FirstOrDefault(),
                        UserRole = _context.Users.Where(u => u.Id == userProfile.UserId).Select(u => u.UserRole).FirstOrDefault(),
                    });
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("User profile updated for user with ID: {UserId}", userProfile.UserId);
                    return Json(new TwitterApiActionResult { Success = true });
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Concurrency exception while updating profile for user with ID: {UserId}", userProfile.UserId);
                    return Json(new TwitterApiActionResult { Success = false, ErrorMessage = "Concurrency exception while updating profile for user with ID: " + userProfile.UserId.ToString() });
                }
                catch (Exception ex)
                {
                    base._logger.LogError(ex, "An error occurred while editing the user profile for user with ID: {UserId}", userProfile.UserId);
                    return Json(new TwitterApiActionResult { Success = false, ErrorMessage = "An error occurred while editing the user profile for user with ID: " + userProfile.UserId.ToString() });
                }
            }
            return Json(new TwitterApiActionResult { Success = false, ErrorMessage = "Invalid input data." });
        }
    }
}