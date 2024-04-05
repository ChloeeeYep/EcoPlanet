using EcoPlanet.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using EcoPlanet.Areas.Identity.Data;

namespace EcoPlanet.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<EcoPlanetUser> _userManager;

        public HomeController(UserManager<EcoPlanetUser> userManager, ILogger<HomeController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Retrieve the current logged-in user
                var user = await _userManager.GetUserAsync(User);

                // Get user type from claims
                char userType = user.UserType; // Assuming UserType is a claim stored during user authentication

                // Redirect based on user type
                switch (userType)
                {
                    case 'A':
                        return RedirectToAction("AdminIndex");
                    case 'D':
                        return RedirectToAction("DriverIndex");
                    default:
                        return View();
                }
            }
            else
            {
                return View();
            }
        }

        public IActionResult AdminIndex()
        {
            return View("AdminIndex");
        }

        public IActionResult DriverIndex()
        {
            return View("DriverIndex");
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
