using Microsoft.AspNetCore.Mvc;
using EcoPlanet.Models;
using EcoPlanet.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Build.Execution;

namespace EcoPlanet.Controllers
{
    public class ProductsController : Controller
    {
        private readonly EcoPlanetContext _context;

        public ProductsController(EcoPlanetContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            List<Products> products = await _context.ProductsTable.ToListAsync();
            return View(products);
        }

        public IActionResult AddProducts()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProducts(Products products)
        {
            if (ModelState.IsValid)
            {
                // Set the CreatedAt property to the current datetime
                products.createdAt = DateTime.Now;

                _context.ProductsTable.Add(products);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(products);
        }

        public async Task<IActionResult> EditProducts(int ? productsId)
        {
            if(productsId == null)
            {
                return NotFound();
            }

            var products = await _context.ProductsTable.FindAsync(productsId);

            if(products == null)
            {
                return BadRequest(productsId + " is not found in the table");
            }
            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProducts(Products products)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    // Set the CreatedAt property to the current datetime
                    products.createdAt = DateTime.Now;

                    _context.ProductsTable.Update(products);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Products");
                }
                return View("EditProducts", products);
            }catch(Exception ex)
            {
                return BadRequest("Error: " + ex.Message);
            }
        }

        public async Task<IActionResult> DeleteProducts(int ? productsId)
        {
            if(productsId == null)
            {
                return NotFound();
            }

            var products = await _context.ProductsTable.FindAsync(productsId);
            if(products == null)
            {
                return BadRequest(productsId + " is not found in the table");
            }
            _context.ProductsTable.Remove(products);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Products");
        }

    }
}
