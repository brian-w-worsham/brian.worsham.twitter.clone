using worsham.twitter.clone.angular.Models;
using worsham.twitter.clone.angular.Models.EntityModels;

namespace worsham.twitter.clone.angular.Services
{
    public interface IAuthenticationService
    {
        Task<Users> AuthenticateUser(string username, string password);
        Task<Users> RegisterUser(Users user, string password);
        void LogoutUser(HttpContext httpContext);
        Task<bool> IsUsernameTaken(string username);
        Task<bool> IsEmailTaken(string email);
    }
}