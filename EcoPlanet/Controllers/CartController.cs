using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcoPlanet.Models;
using System.Security.Claims;
using EcoPlanet.Data;
using Microsoft.AspNetCore.Identity;
using EcoPlanet.Areas.Identity.Data;
using Amazon; //account purpose
using Amazon.S3; //creating s3 process
using Amazon.S3.Model; //object structure in S3
using Microsoft.Extensions.Configuration; //accessing apps.settings
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using EcoPlanet.ViewModels; //for object data transmissions in network

namespace EcoPlanet.Controllers
{
    public class CartController : Controller
    {
        private const string bucketname = "ecoplanet";

        private readonly EcoPlanetContext _context;
        private readonly UserManager<EcoPlanetUser> _userManager;

        //function 1:create a function to retrive the keys back from json file
        private List<string> getKeys()
        {
            //1.1 create empty lists for storing keys
            List<string> keylist = new List<string>();

            //1.2 Get the keys back from json
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");

            IConfigurationRoot conf = builder.Build();

            //1.3 Add the retrived keys to the list
            keylist.Add(conf["Keys:Key1"]);
            keylist.Add(conf["Keys:Key2"]);
            keylist.Add(conf["Keys:Key3"]);

            //1.4 return the key lists
            return keylist;
        }

        public CartController(EcoPlanetContext context, UserManager<EcoPlanetUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // GET: Cart
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var maxQuantities = new Dictionary<int, int>();

            if (user == null)
            {
                return Challenge(); // Prompt the user to log in
            }

            // Retrieve the cart items for the user
            var cartItems = await _context.CartItemTable
                                          .Where(c => c.Cart.userId == user.Id)
                                          .ToListAsync();

            // Generate the image URLs for each cart item
            var imageUrls = await GetImageUrlsForCartItems(cartItems);

            foreach (var cartItem in cartItems) // This line has been changed from cart.Items to cartItems
            {
                // Assuming cartItem has a property goodsId that corresponds to goodsId in GoodsTable
                var goods = await _context.GoodsTable
                                          .AsNoTracking()
                                          .FirstOrDefaultAsync(g => g.goodsId == cartItem.goodsId);
                if (goods != null)
                {
                    maxQuantities[cartItem.goodsId] = goods.goodsQuantity;
                }
            }

            // Prepare the view model
            var viewModel = new CartViewModel
            {
                Cart = new Cart { Items = cartItems }, // Use the cartItems variable here directly
                ImageUrls = imageUrls,
                MaxQuantities = maxQuantities
            };

            return View(viewModel);
        }


        private async Task<Dictionary<string, string>> GetImageUrlsForCartItems(IEnumerable<CartItem> items)
        {
            // Connect to the AWS account
            List<string> keys = getKeys();
            var s3Client = new AmazonS3Client(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);
            var imageUrls = new Dictionary<string, string>();

            foreach (var item in items)
            {
                string key = item.goodsImage?.Trim();
                if (!string.IsNullOrEmpty(key))
                {
                    // Construct the image URL
                    string imageUrl = $"https://{bucketname}.s3.amazonaws.com/{key}";
                    imageUrls[key] = imageUrl;
                }
            }

            return imageUrls;
        }



        private async Task<Cart> GetOrCreateCartForUser()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            var userId = user.Id; // Keep it as string if your IDs are strings

            var cart = await _context.CartTable
                                     .Include(c => c.Items)
                                     .FirstOrDefaultAsync(c => c.userId == userId);

            if (cart == null)
            {
                cart = new Cart { userId = userId };
                _context.CartTable.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int goodsId, int quantity)
        {
            var goods = await _context.GoodsTable.FirstOrDefaultAsync(g => g.goodsId == goodsId);

            if (goods == null)
            {
                return NotFound();
            }

            //1.connect to the AWS account
            List<string> keys = getKeys();
            AmazonS3Client agent = new AmazonS3Client(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);

            //2. create empty lists that can store the retrieved images from S3
            List<S3Object> imagelist = new List<S3Object>();

            //3.read image by image and store to the lists
            string? nextToken = null;
            do
            {
                //3.1 Create Lists Request
                ListObjectsRequest request = new ListObjectsRequest
                {
                    BucketName = bucketname
                };

                //3.2 execute the request
                ListObjectsResponse response = await agent.ListObjectsAsync(request);

                //3.3 Store the images from response to the list
                imagelist.AddRange(response.S3Objects);

                //3.4 check the next addressing and store into the next token
                nextToken = response.NextMarker;

            }
            while (nextToken != null);

            var cart = await GetOrCreateCartForUser();
            var cartItem = cart.Items.FirstOrDefault(ci => ci.goodsId == goodsId);

            if (cartItem != null)
            {
                // If the item already exists in the cart, just update the quantity
                cartItem.goodsQuantity += quantity;
            }
            else
            {
                // Otherwise, create a new CartItem and add it to the Cart
                cartItem = new CartItem
                {
                    goodsId = goods.goodsId,
                    goodsName = goods.goodsName,
                    goodsQuantity = quantity,
                    goodsPrice = goods.goodsPrice,
                    goodsImage = goods.goodsImage, // Adjust the default image path as necessary
                    Cart = cart
                };

                cart.Items.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index)); // Redirect to the cart index view
        }

        // POST: Cart/UpdateQuantity/5
        [HttpPost]
        public IActionResult UpdateQuantity(int cartItemId, int quantity)
        {
            var cartItem = _context.CartItemTable.Find(cartItemId);
            if (cartItem != null)
            {
                cartItem.goodsQuantity = quantity;
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/RemoveFromCart/5
        [HttpPost]
        public IActionResult RemoveFromCart(int cartItemId)
        {
            var cartItem = _context.CartItemTable.Find(cartItemId);
            if (cartItem != null)
            {
                _context.CartItemTable.Remove(cartItem);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckout(CheckoutViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            // Start a transaction in case anything goes wrong you can rollback
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Create and populate the order from the model
                    var order = new Order
                    {
                        Email = user.Email,
                        OrderDate = DateTime.UtcNow,
                        Contact = model.PhoneNumber,
                        Address = model.Address,
                        PaymentMethod = "Credit Card/Debit Card",
                        PaymentStatus = "Pending",
                        OrderStatus = "In Progress"
                    };

                    // Get the cart items
                    var cartItems = await _context.CartItemTable
                                                  .Where(c => c.Cart.userId == user.Id)
                                                  .ToListAsync();

                    // Convert cart items to order items
                    foreach (var cartItem in cartItems)
                    {
                        var orderItem = new OrderItem
                        {
                            goodsId = cartItem.goodsId,
                            goodsName = cartItem.goodsName,
                            goodsQuantity = cartItem.goodsQuantity,
                            goodsPrice = cartItem.goodsPrice,
                            SellerId = user.Id
                        };

                        order.OrderItems.Add(orderItem);
                    }

                    // Save the order to the database
                    _context.OrderTable.Add(order);
                    await _context.SaveChangesAsync();

                    // Clear the cart after order placement
                    _context.CartItemTable.RemoveRange(cartItems);
                    await _context.SaveChangesAsync();

                    // Commit the transaction
                    transaction.Commit();

                    // Redirect to a confirmation page or similar
                    return RedirectToAction("OrderConfirmation", new { orderId = order.OrderId });
                }
                catch (Exception ex)
                {
                    // Log the exception
                    transaction.Rollback();
                    // Handle the error, maybe return a view with an error message
                    return View("Error");
                }
            }
        }


        public async Task<IActionResult> OrderConfirmation(int? orderId)
        {
            if (orderId == null)
            {
                return NotFound();
            }

            var order = await _context.OrderTable
                                      .Include(o => o.OrderItems)
                                      .FirstOrDefaultAsync(m => m.OrderId == orderId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}
