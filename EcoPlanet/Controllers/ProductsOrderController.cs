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



        //Admin Order

        public async Task<IActionResult> Index()
        {
            // Fetch orders and include the user's full name for drivers
            var userOrdersWithDriverName = await _context.ProductsOrderTable
                .Include(o => o.ProductsOrderItems)
                .Select(o => new {
                    ProductsOrderId = o.ProductsOrderId,
                    Email = o.Email,
                    TotalPrice = o.TotalPrice, // Make sure these property names match your entity
                    OrderStatus = o.OrderStatus,
                    OrderDate = o.ProductsOrderDate, // Correct property name
                    DriverFullName = _context.Users.Where(u => u.Id == o.DriverId).Select(u => u.FullName).FirstOrDefault() ?? "N/A"
                })
                .OrderByDescending(o => o.OrderDate) // Correct property name
                .ToListAsync();

            // Pass the data to the view using ViewData
            ViewData["Orders"] = userOrdersWithDriverName;

            return View();
        }



        public async Task<IActionResult> ManageOrdersDetails(int orderId)
        {
            // Get the order details for the specified order ID, including the driver's full name
            var orderDetails = await _context.ProductsOrderTable
                                             .Where(o => o.ProductsOrderId == orderId)
                                             .Include(o => o.ProductsOrderItems)
                                             .Select(o => new
                                             {
                                                 Order = o, // Preserve the original order details
                                                 DriverFullName = _context.Users
                                                                   .Where(u => u.Id == o.DriverId)
                                                                   .Select(u => u.FullName)
                                                                   .FirstOrDefault() // Retrieve the driver's full name
                                             })
                                             .FirstOrDefaultAsync();

            if (orderDetails?.Order == null) // Check if the result or the Order is null
            {
                return NotFound(); // Or another appropriate response
            }

            // Construct the base URL for the S3 bucket and pass it to the view
            var baseUrl = $"https://{bucketname}.s3.amazonaws.com/";
            ViewData["BaseUrl"] = baseUrl;

            // Pass the driver's full name to the view through ViewData or ViewBag
            ViewData["DriverFullName"] = orderDetails.DriverFullName;

            // Return the view with the order's details
            return View(orderDetails.Order);
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

			var drivers = await _context.Users
	           .Where(u => u.UserType == 'D')
	           .Select(u => new { u.Id, u.FullName })
	           .ToListAsync();

			// Passing the list of drivers to the view using ViewBag
			ViewBag.Drivers = drivers;

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




        //User Order
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
            // Get the order details for the specified order ID, including the driver's full name
            var orderDetails = await _context.ProductsOrderTable
                                             .Where(o => o.ProductsOrderId == orderId)
                                             .Include(o => o.ProductsOrderItems)
                                             .Select(o => new
                                             {
                                                 Order = o, // Preserve the original order details
                                                 DriverFullName = _context.Users
                                                                   .Where(u => u.Id == o.DriverId)
                                                                   .Select(u => u.FullName)
                                                                   .FirstOrDefault() // Retrieve the driver's full name
                                             })
                                             .FirstOrDefaultAsync();

            if (orderDetails?.Order == null) // Check if the result or the Order is null
            {
                return NotFound(); // Or another appropriate response
            }

            // Construct the base URL for the S3 bucket and pass it to the view
            var baseUrl = $"https://{bucketname}.s3.amazonaws.com/";
            ViewData["BaseUrl"] = baseUrl;

            // Pass the driver's full name to the view through ViewData or ViewBag
            ViewData["DriverFullName"] = orderDetails.DriverFullName;

            // Return the view with the order's details
            return View(orderDetails.Order);
        }



        //Drive Order

        public async Task<IActionResult> DriverIndex()
        {
            // Get the current logged-in user's Id
            var currentUserId = _userManager.GetUserId(User); // This gets the logged-in user's Id

            // Fetch orders assigned to the current logged-in driver and include the user's full name for drivers
            var userOrdersWithDriverName = await _context.ProductsOrderTable
                .Include(o => o.ProductsOrderItems)
                .Where(o => o.DriverId == currentUserId) // Filter by the current driver's Id
                .Select(o => new {
                    ProductsOrderId = o.ProductsOrderId,
                    Email = o.Email,
                    TotalPrice = o.TotalPrice,
                    OrderStatus = o.OrderStatus,
                    OrderDate = o.ProductsOrderDate,
                    // Since we're filtering by driver, we know this is the current user
                    DriverFullName = _context.Users.Where(u => u.Id == o.DriverId).Select(u => u.FullName).FirstOrDefault() ?? "N/A"
                })
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // Pass the data to the view using ViewData
            ViewData["Orders"] = userOrdersWithDriverName;

            return View();
        }

        public async Task<IActionResult> DriverOrdersDetails(int orderId)
        {
            // Get the order details for the specified order ID, including the driver's full name
            var orderDetails = await _context.ProductsOrderTable
                                             .Where(o => o.ProductsOrderId == orderId)
                                             .Include(o => o.ProductsOrderItems)
                                             .Select(o => new
                                             {
                                                 Order = o, // Preserve the original order details
                                                 DriverFullName = _context.Users
                                                                   .Where(u => u.Id == o.DriverId)
                                                                   .Select(u => u.FullName)
                                                                   .FirstOrDefault() // Retrieve the driver's full name
                                             })
                                             .FirstOrDefaultAsync();

            if (orderDetails?.Order == null) // Check if the result or the Order is null
            {
                return NotFound(); // Or another appropriate response
            }

            // Construct the base URL for the S3 bucket and pass it to the view
            var baseUrl = $"https://{bucketname}.s3.amazonaws.com/";
            ViewData["BaseUrl"] = baseUrl;

            // Pass the driver's full name to the view through ViewData or ViewBag
            ViewData["DriverFullName"] = orderDetails.DriverFullName;

            // Return the view with the order's details
            return View(orderDetails.Order);
        }

        public async Task<IActionResult> DriverEditOrders(int? orderId)
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

            var drivers = await _context.Users
               .Where(u => u.UserType == 'D')
               .Select(u => new { u.Id, u.FullName })
               .ToListAsync();

			// Find the full name of the driver for this order
			var driverName = drivers.FirstOrDefault(d => d.Id == orders.DriverId)?.FullName;

			// Pass the full name to the view using ViewBag
			ViewBag.DriverFullName = driverName;

			// Passing the list of drivers to the view using ViewBag (if needed for other purposes)
			ViewBag.Drivers = drivers;

			return View(orders);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DriverUpdateOrders(int productsOrderId, string orderStatus)
		{
			var orderToUpdate = await _context.ProductsOrderTable
											  .FirstOrDefaultAsync(o => o.ProductsOrderId == productsOrderId);

			if (orderToUpdate == null)
			{
				return NotFound();
			}

			// Update the order status with the new value
			orderToUpdate.OrderStatus = orderStatus;

			// Flag the OrderStatus property as modified
			_context.Entry(orderToUpdate).Property(o => o.OrderStatus).IsModified = true;

			// Save the changes to the database
			await _context.SaveChangesAsync();

			// Redirect back to a safe page, such as the order details or order list
			return RedirectToAction("DriverIndex", "ProductsOrder");
		}

	}
}