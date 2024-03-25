using Microsoft.AspNetCore.Mvc;
using EcoPlanet.Models;
using EcoPlanet.Data;
using Microsoft.EntityFrameworkCore;


namespace EcoPlanet.Controllers
{
    public class TrashpediaController : Controller
    {
        private readonly EcoPlanetContext _context;

        public TrashpediaController(EcoPlanetContext context) 
        { 
            _context = context; 
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddData()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddData(Trashpedia trashpedia)
        {
            if (ModelState.IsValid)
            {
                _context.Add(trashpedia);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(trashpedia);
        }


    }
}
