namespace worsham.twitter.clone.Services
{
    public interface IAuthorizationService
    {
        bool Authorize(string requiredRole, ISession? session);
    }
}