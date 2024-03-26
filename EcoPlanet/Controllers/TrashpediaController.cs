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

        public async Task<IActionResult> Index() 
        { 
            List<Trashpedia> trashpedias = await _context.TrashpediaTable.ToListAsync(); return View(trashpedias);
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
                // Set the CreatedAt property to the current datetime
                trashpedia.CreatedAt = DateTime.Now;

                _context.TrashpediaTable.Add(trashpedia);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(trashpedia);
        }

        public async Task<IActionResult> EditData(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            var trashpedia = await _context.TrashpediaTable.FindAsync(Id);

            if (trashpedia == null)
            {
                return BadRequest(Id + " is not found in the table!");
            }

            return View(trashpedia);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateData(Trashpedia trashpedia)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.TrashpediaTable.Update(trashpedia);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Trashpedia");
                }
                else
                {
                    return View("EditData", trashpedia);
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Error: " + ex.Message);
            }
        }


        public async Task<IActionResult> DeleteData(int? Id)
        {
            try
            {
                if (Id == null)
                {
                    return NotFound();
                }

                var trashpedia = await _context.TrashpediaTable.FindAsync(Id);

                if (trashpedia == null)
                {
                    return NotFound();
                }

                _context.TrashpediaTable.Remove(trashpedia);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Trashpedia");
            }
            catch (Exception ex)
            {
                return BadRequest("Error deleting trashpedia: " + ex.Message);
            }
        }





    }
}
