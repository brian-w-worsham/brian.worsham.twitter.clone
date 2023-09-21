namespace worsham.twitter.clone.angular.Services
{
    public interface IAuthorizationService
    {
        bool Authorize(string requiredRole, ISession? session);
    }
}