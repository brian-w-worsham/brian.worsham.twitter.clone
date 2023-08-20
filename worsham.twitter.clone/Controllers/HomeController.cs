using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using worsham.twitter.clone.Models;

namespace worsham.twitter.clone.Controllers
{
    public class HomeController : Controller
    {
        private readonly TwitterCloneContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(TwitterCloneContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Displays the landing page or redirects to the Tweets page based on the user's authentication status.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> representing the landing page or a redirection to the Tweets page.</returns>
        public IActionResult Index()
        {
            // Check if the user is authenticated based on the presence of the "UserId" in the session
            bool isAuthenticated = HttpContext.Session.GetInt32("UserId") != null;
            if (isAuthenticated)
            {
                _logger.LogInformation("User is authenticated. Redirecting to Tweets.");
                return RedirectToAction(actionName: "Index", controllerName: "Tweets");
            }
            else
            {
                _logger.LogInformation("User is not authenticated. Displaying the landing page.");
                return View(model: new Users());
            }
        }

        /// <summary>
        /// Displays the sign-in modal and sets a flag to indicate that the user has registered.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> representing the view with the sign-in modal displayed.</returns>
        public IActionResult DisplaySignInModal()
        {
            _logger.LogInformation("Displaying sign-in modal.");
            ViewData["DidUserRegister"] = true;
            return View(viewName: "Index", model: new Users());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}