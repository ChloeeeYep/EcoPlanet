using Microsoft.AspNetCore.Mvc;
using EcoPlanet.Models;
using EcoPlanet.Data;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Amazon; //for linking your AWS account
using Amazon.S3;
using Amazon.S3.Model;
using EcoPlanet.ViewModels;
using System.Linq;
using System;
using EcoPlanet.Areas.Identity.Data;
using EcoPlanet.Data;

namespace EcoPlanet.Controllers
{
    public class ProductsOrderController : Controller
    {

        private readonly EcoPlanetContext _context;
        private readonly UserManager<EcoPlanetUser> _userManager;
        private const string bucketname = "ecoplanet";

        public ProductsOrderController(EcoPlanetContext context, UserManager<EcoPlanetUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //Create a function to retrieve the keys back from json file
        private List<string> getKeys()
        {
            //1.1 create emppty list for storing the keys
            List<string> keylist = new List<string>();

            //1.2 get the keys back from json
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            IConfiguration conf = builder.Build(); //build the file

            //1.3 add the retrieved to the list
            keylist.Add(conf["keys:key1"]);
            keylist.Add(conf["keys:key2"]);
            keylist.Add(conf["keys:key3"]);

            //1.4 return the key list
            return keylist;
        }

        public async Task<IActionResult> Index()
        {
            // Fetch orders for the user based on their email
            var userOrders = await _context.ProductsOrderTable                                        
                                           .Include(o => o.ProductsOrderItems)
                                           .OrderByDescending(o => o.ProductsOrderDate)
                                           .ToListAsync();

            // Return the view with the user's orders
            return View(userOrders);
        }

        public async Task<IActionResult> ManageOrdersDetails(int orderId)
        {
            // Get the order details for the specified order ID
            var orderDetails = await _context.ProductsOrderTable
                                             .Where(o => o.ProductsOrderId == orderId)
                                             .Include(o => o.ProductsOrderItems)
                                             .FirstOrDefaultAsync();

            if (orderDetails == null)
            {
                return NotFound(); // Or another appropriate response
            }

            // Construct the base URL for the S3 bucket and pass it to the view
            var baseUrl = $"https://{bucketname}.s3.amazonaws.com/";
            ViewData["BaseUrl"] = baseUrl;

            // Return the view with the order's details
            return View(orderDetails);
        }


        public async Task<IActionResult> EditOrders(int ? orderId)
        {
            if (orderId == null)
            {
                return NotFound();
            }
            var orders = await _context.ProductsOrderTable.FindAsync(orderId);

            if (orders == null)
            {
                return BadRequest(orderId + " is not found in the table");
            }
            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrders(int productsOrderId, string? driverId)
        {
            var orderToUpdate = await _context.ProductsOrderTable
                                              .FirstOrDefaultAsync(o => o.ProductsOrderId == productsOrderId);

            if (orderToUpdate == null)
            {
                return NotFound();
            }

            // Assign the new driver id, which could be null or a string
            orderToUpdate.DriverId = driverId;

            // Update the order status based on the presence or absence of the driver id
            orderToUpdate.OrderStatus = string.IsNullOrEmpty(driverId) ? "In Progress" : "Delivering";

            // Flag the properties you want to update
            _context.Entry(orderToUpdate).Property(o => o.DriverId).IsModified = true;
            _context.Entry(orderToUpdate).Property(o => o.OrderStatus).IsModified = true;

            // Save the changes to the database
            await _context.SaveChangesAsync();

            // Redirect back to a safe page, such as the order details or order list
            return RedirectToAction("Index", "ProductsOrder");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrders(int orderId)
        {
            var order = await _context.ProductsOrderTable
                                  .FirstOrDefaultAsync(o => o.ProductsOrderId == orderId);

            if (order == null)
            {
                return NotFound();
            }

            order.DriverId = null; // Set the DriverId property to null
            order.OrderStatus = "Canceled";

            // You only need to mark the OrderStatus as modified
            _context.Entry(order).Property(o => o.OrderStatus).IsModified = true;
            _context.Entry(order).Property(o => o.DriverId).IsModified = true;

            await _context.SaveChangesAsync();

            // Redirect to the index action of ProductsOrderController or any other appropriate action
            return RedirectToAction("Index", "ProductsOrder");
        }


        public async Task<IActionResult> ShowOrders()
        {
            // Get the current logged-in user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge(); // Prompt the user to log in if not logged in
            }

            // Fetch orders for the user based on their email
            var userOrders = await _context.ProductsOrderTable
                                           .Where(o => o.Email == user.Email)
                                           .Include(o => o.ProductsOrderItems)
                                           .OrderByDescending(o => o.ProductsOrderDate)
                                           .ToListAsync();

            // Construct the base URL for the S3 bucket and pass it to the view
            var baseUrl = $"https://{bucketname}.s3.amazonaws.com/";
            ViewData["BaseUrl"] = baseUrl;

            // Return the view with the user's orders
            return View(userOrders);
        }

        public async Task<IActionResult> ShowOrdersDetails(int orderId)
        {
            // Get the order details for the specified order ID
            var orderDetails = await _context.ProductsOrderTable
                                             .Where(o => o.ProductsOrderId == orderId)
                                             .Include(o => o.ProductsOrderItems)
                                             .FirstOrDefaultAsync();

            if (orderDetails == null)
            {
                return NotFound(); // Or another appropriate response
            }

            // Construct the base URL for the S3 bucket and pass it to the view
            var baseUrl = $"https://{bucketname}.s3.amazonaws.com/";
            ViewData["BaseUrl"] = baseUrl;

            // Return the view with the order's details
            return View(orderDetails);
        }
    }
}