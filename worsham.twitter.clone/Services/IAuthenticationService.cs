using worsham.twitter.clone.Models;
using worsham.twitter.clone.Models.EntityModels;

namespace worsham.twitter.clone.Services
{
    public interface IAuthenticationService
    {
        Task<Users> AuthenticateUser(string username, string password);
        Task<Users> RegisterUser(Users user, string password);
        void LogoutUser(HttpContext httpContext);
        Task<bool> IsUsernameTaken(string username);
    }
}