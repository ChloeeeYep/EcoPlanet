using Microsoft.AspNetCore.Mvc;
using EcoPlanet.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EcoPlanet.ViewModels;
using System.Linq;
using System;
using EcoPlanet.Areas.Identity.Data;
using EcoPlanet.Data;


namespace EcoPlanet.Controllers
{
    public class OrderController : Controller
    {
        private readonly EcoPlanetContext _context;
        private readonly UserManager<EcoPlanetUser> _userManager;

        public OrderController(EcoPlanetContext context, UserManager<EcoPlanetUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Check if the user is an admin or a seller, then retrieve orders accordingly
            if (User.IsInRole("A"))
            {
                // Admin gets to see all orders
                var allOrders = await _context.OrderTable.Include(o => o.OrderItems).ToListAsync();
                return View(allOrders);
            }
            else if (User.IsInRole("S"))
            {
                // Sellers only see orders that contain their products
                var sellerOrders = await _context.OrderTable
                                                 .Where(o => o.OrderItems.Any(oi => oi.SellerId == user.Id))
                                                 .ToListAsync();
                return View(sellerOrders);
            }
            else
            {
                // Regular users see their own orders
                var userOrders = await _context.OrderTable.Where(o => o.Email == user.Email).ToListAsync();
                return View(userOrders);
            }
        }

        // GET: /Order/Details/X
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.OrderTable.Include(o => o.OrderItems)
                                             .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null) return NotFound();

            return View(order);
        }

        // POST: /Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CartId, UserId, ...")] Order order)
        {
            if (ModelState.IsValid)
            {
                order.OrderDate = DateTime.UtcNow;
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }

        // GET: /Order/Edit/5
        public async Task<IActionResult> Edit(int ? id)
        {
            if (id == null) return NotFound();

            var order = await _context.OrderTable.FindAsync(id);
            if (order == null) return NotFound();

            return View(order);
        }

        // POST: /Order/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,UserId,...")] Order order)
        {
            if (id != order.OrderId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }

        // GET: /Order/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.OrderTable.FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null) return NotFound();

            return View(order);
        }

        // POST: /Order/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.OrderTable.FindAsync(id);
            _context.OrderTable.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.OrderTable.Any(e => e.OrderId == id);
        }


    }
}
