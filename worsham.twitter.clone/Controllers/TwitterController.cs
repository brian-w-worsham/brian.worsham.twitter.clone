using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using worsham.twitter.clone.Services;
using Microsoft.AspNetCore.Mvc.Filters;

namespace worsham.twitter.clone.Controllers
{
    public class TwitterController : Controller
    {
        protected readonly ILogger<TwitterController> _logger;
        protected readonly IAuthorizationService _authorizationService;
        protected int? _currentUserId;
        protected ISession? _session;

        public TwitterController(ILogger<TwitterController> logger, IAuthorizationService authorizationService)
        {
            _logger = logger;
            _authorizationService = authorizationService;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _currentUserId = HttpContext.Session.GetInt32("UserId");
            _session = HttpContext.Session;
        }


        protected IActionResult RedirectIfNotAdmin()
        {
            bool isUserAuthorized = _authorizationService.Authorize("admin", _session);
            if (isUserAuthorized == false)
            {
                // Check whether the user is logged in. If they are not logged in redirect them to
                // the Home/Index view.
                if (_currentUserId == null)
                {
                    _logger.LogInformation("User is not logged in. Redirecting to Home/Index.");
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    _logger.LogInformation("User is not authorized to view comments. Redirecting to Tweets/Index.");
                    return RedirectToAction("Index", "Tweets");
                }
            }
            else
            {
                return null;
            }
        }
    }
}
