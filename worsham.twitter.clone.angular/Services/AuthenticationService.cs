using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Authentication;
using worsham.twitter.clone.angular.Models;
using worsham.twitter.clone.angular.Models.EntityModels;

namespace worsham.twitter.clone.angular.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly TwitterCloneContext _context;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(TwitterCloneContext context, ILogger<AuthenticationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Authenticates a user based on the provided username and password.
        /// </summary>
        /// <param name="username">The username of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>The authenticated user if successful, or throws an exception if authentication fails.</returns>
        /// <exception cref="ArgumentException">Thrown when the username or password is null or empty.</exception>
        /// <exception cref="AuthenticationException">Thrown when the provided username does not correspond to an existing user or when the password is incorrect.</exception>
        public async Task<Users> AuthenticateUser(string username, string password)
        {
            // Log the authentication attempt
            _logger.LogInformation("Authentication attempt for username: {Username}", username);

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                _logger.LogWarning("Authentication failed due to missing username or password.");
                throw new ArgumentException("Username and password are required.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == username);

            if (user == null)
            {
                _logger.LogWarning("Authentication failed. User not found for username: {Username}", username);
                throw new AuthenticationException("User not found.");
            }

            // check if password is correct
            if (!VerifyPasswordHash(password, user.Password))
            {
                _logger.LogWarning("Authentication failed for username: {Username}. Invalid password.", username);
                throw new AuthenticationException("Invalid username or password.");
            }

            _logger.LogInformation("Authentication successful for username: {Username}", username);
            return user;
        }

        /// <summary>
        /// Verifies whether the provided password matches the stored password hash.
        /// </summary>
        /// <param name="password">The password to be verified.</param>
        /// <param name="storedHash">The stored password hash to compare against.</param>
        /// <returns>True if the password matches the stored hash, otherwise false.</returns>
        /// <remarks>This method uses BCrypt hashing algorithm for password verification.</remarks>
        private bool VerifyPasswordHash(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }


        /// <summary>
        /// Registers a new user with the provided information and password.
        /// </summary>
        /// <param name="user">The user information to be registered.</param>
        /// <param name="password">The password for the new user.</param>
        /// <returns>The registered user object.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the user parameter is null.</exception>
        /// <exception cref="Exception">Thrown if the username or email is already taken, or if the password is missing.</exception>
        /// <remarks>
        /// This method registers a new user by validating the input, hashing the password,
        /// and saving the user to the database using the provided context. If the registration
        /// is successful, it logs the registration event. If an exception occurs during the registration,
        /// it logs the error along with the failed username and rethrows the exception.
        /// </remarks>
        public async Task<Users> RegisterUser(Users user, string password)
        {
            try
            {
                await ValidateUserInput(user, password);

                user.Password = HashPassword(password);
                user.UserRole = "user"; // default role is user

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User registered successfully: {Username}", user.UserName);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User registration failed for username: {Username}", user.UserName);
                throw;
            }
        }

        /// <summary>
        /// Hashes the provided password using BCrypt encryption.
        /// </summary>
        /// <param name="password">The password to be hashed.</param>
        /// <returns>The hashed password.</returns>
        /// <remarks>
        /// This method securely hashes the provided password using BCrypt encryption,
        /// which enhances the security of user credentials by generating a hash that is difficult to reverse-engineer.
        /// </remarks>
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// Validates user input for registration.
        /// </summary>
        /// <param name="user">The user object containing registration details.</param>
        /// <param name="password">The password provided for registration.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous validation process.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the user object is null.</exception>
        /// <exception cref="Exception">Thrown when the password is empty or whitespace, or when the username or email is already taken.</exception>
        /// <remarks>
        /// This method performs validation on the user registration input, checking for null user objects,
        /// empty or whitespace passwords, and duplicate usernames or email addresses in the database.
        /// If any validation fails, an exception is thrown to indicate the specific validation issue.
        /// </remarks>
        private async Task ValidateUserInput(Users user, string password)
        {
            if (user == null)
            {
                _logger.LogWarning("User registration failed due to null user object.");
                throw new ArgumentNullException(nameof(user));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("User registration failed due to missing password.");
                throw new Exception("Password is required");
            }

            // Check if the username is already taken
            bool isUsernameTaken = await IsUsernameTaken(user.UserName);
            if (isUsernameTaken)
            {
                _logger.LogWarning("User registration failed. Username {Username} is already taken.", user.UserName);
                throw new InvalidOperationException($"Username \"{user.UserName}\" is already taken");
            }


            if (await _context.Users.AnyAsync(x => x.Email == user.Email))
            {
                _logger.LogWarning("User registration failed. Email {Email} is already taken.", user.Email);
                throw new Exception("Email \"" + user.Email + "\" is already taken");
            }
        }

        /// <summary>
        /// Checks if a username is already taken by an existing user.
        /// </summary>
        /// <param name="username">The username to check for availability.</param>
        /// <returns>
        /// True if the username is already taken; otherwise, false.
        /// </returns>
        /// <remarks>
        /// This method queries the database to determine if the provided username
        /// is already associated with an existing user. It returns true if the username
        /// is found, indicating that the username is not available for registration.
        /// Otherwise, it returns false, indicating that the username is available.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the provided username is null or empty.
        /// </exception>
        /// <seealso cref="RegisterUser(Users, string)"/>
        public async Task<bool> IsUsernameTaken(string username)
        {
            return await _context.Users.AnyAsync(u => u.UserName == username);
        }

        /// <summary>
        /// Checks if the given email is already taken by a user in the database.
        /// </summary>
        /// <param name="email">The email to check.</param>
        /// <returns>True if the email is taken, false otherwise.</returns>
        public async Task<bool> IsEmailTaken(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        /// <summary>
        /// Clears the user's session, effectively logging them out.
        /// </summary>
        /// <param name="httpContext">The HttpContext representing the user's session context.</param>
        /// <remarks>
        /// This method clears the user's session, effectively logging them out from the application.
        /// It removes all session data associated with the user, providing a secure way to end their session.
        /// </remarks>
        public void LogoutUser(HttpContext httpContext)
        {
            httpContext.Session.Clear();
        }
    }
}
