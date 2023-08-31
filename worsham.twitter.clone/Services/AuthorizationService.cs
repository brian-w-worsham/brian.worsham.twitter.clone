using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace worsham.twitter.clone.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly ILogger<AuthorizationService> _logger;

        public AuthorizationService(ILogger<AuthorizationService> logger)
        {
            _logger = logger;
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
    }
}
