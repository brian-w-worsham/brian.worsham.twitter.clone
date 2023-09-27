using worsham.twitter.clone.angular.Models.EntityModels;

namespace worsham.twitter.clone.angular.Services
{
    public interface IAuthorizationService
    {
        bool Authorize(string requiredRole, ISession? session);
        Task<Users> GetAuthenticatedUserAsync(string authorizationHeader);
    }
}