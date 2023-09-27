using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using worsham.twitter.clone.angular.Models.EntityModels;

namespace worsham.twitter.clone.angular.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly TwitterCloneContext _context;
        private readonly ILogger<AuthorizationService> _logger;
        private readonly IConfiguration _configuration;

        public AuthorizationService(TwitterCloneContext context, ILogger<AuthorizationService> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Checks if the current user has the required role.
        /// </summary>
        /// <param name="requiredRole">The role required to access a resource.</param>
        /// <returns>True if the user has the required role; otherwise, false.</returns>
        public bool Authorize(string requiredRole, ISession? session)
        {
            string? userRole = session.GetString("UserRole");
            int? currentUserId = session.GetInt32("UserId");
            var userName = session.GetString("UserName");

            if (currentUserId == null)
            {
                _logger.LogWarning("Authorization failed. User is not authenticated.");
                return false;
            }

            if ((!string.IsNullOrWhiteSpace(userRole)) && userRole == requiredRole)
            {
                _logger.LogInformation("Authorization successful for user {Username}. Required role: {Role}", userName, requiredRole);
                return true;
            }

            _logger.LogWarning("Authorization failed for user {Username}. Required role: {Role}", userName, requiredRole);
            return false;
        }

        public async Task<Users> GetAuthenticatedUserAsync(string authorizationHeader)
        {

            // Retrieve the JWT token from the Authorization header
            var token = authorizationHeader?.Split(' ').Last();

            // retrieve SecretKeyForJwtToken from secrets.json
            var secretKey = _configuration["SecretKeyForJwtToken"];
            var key = secretKey != null ? Encoding.ASCII.GetBytes(secretKey) : throw new ArgumentNullException(nameof(secretKey));

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
                ValidateIssuer = false,
                ValidateAudience = false
            };

            SecurityToken validatedToken;
            var claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);

            // Validate the JWT token and extract the user's claims
            var userIdClaim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            int userId = int.Parse(userIdClaim?.Value ?? throw new Exception("userIdClaim is null"));
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            return user;
        }
    }
}
