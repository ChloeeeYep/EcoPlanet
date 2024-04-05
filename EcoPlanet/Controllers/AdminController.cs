using EcoPlanet.Areas.Identity.Data;
using EcoPlanet.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcoPlanet.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<EcoPlanetUser> _userManager;
        private readonly EcoPlanetContext _context;

        public AdminController(UserManager<EcoPlanetUser> userManager, EcoPlanetContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            if (users == null)
            {
                // Handle the case where users are null
                return NotFound("No users found.");
            }
            return View(users);
        }


        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{id}'.");
            }

            // Perform the deletion
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                // Handle the case where the deletion failed
                throw new InvalidOperationException($"Unexpected error occurred deleting user with ID '{id}'.");
            }

            // Redirect to the list of users after successful deletion
            return RedirectToAction("Index","Admin");
        }


    }
}
