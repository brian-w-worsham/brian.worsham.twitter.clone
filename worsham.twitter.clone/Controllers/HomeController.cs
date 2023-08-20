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

        public async Task<IActionResult> Index()
        {
            bool IsAuthenticated = false;
            if (IsAuthenticated)
            {
                return RedirectToAction("Index", "Tweets");
            }
            else
            {
                return View(model: new Users());
            }
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