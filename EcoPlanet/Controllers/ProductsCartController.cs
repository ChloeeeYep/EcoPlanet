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
    public class ProductsCartController : Controller
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

        public ProductsCartController(EcoPlanetContext context, UserManager<EcoPlanetUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var productsCartItems = new List<ProductsCartItem>();
            List<int> removedItemsIds = new List<int>();
            var maxQuantities = new Dictionary<int, int>();

            if (user == null)
            {
                return Challenge(); // Prompt the user to log in
            }

            // Retrieve the cart items for the user
            var productsCartItem = await _context.ProductsCartItemTable
                                          .Where(c => c.ProductsCart.userId == user.Id)
                                          .ToListAsync();

            foreach (var item in productsCartItem)
            {
                var products = await _context.ProductsTable
                                          .AsNoTracking()
                                          .FirstOrDefaultAsync(g => g.productsId == item.productsId);
                if (products != null && products.productsQuantity > 0)
                {
                    productsCartItems.Add(item);
                }
                else
                {
                    _context.ProductsCartItemTable.Remove(item); // Remove the item from the cart
                    removedItemsIds.Add(item.productsId);
                }
            }

            // Save changes if any items were removed
            if (removedItemsIds.Count > 0)
            {
                await _context.SaveChangesAsync();
                TempData["RemovedItems"] = $"Some items were removed from your cart as they are no longer available.";
            }


            // Generate the image URLs for each cart item
            var imageUrls = await GetImageUrlsForCartItems(productsCartItem);


            foreach (var item in productsCartItems)
            {
                maxQuantities[item.productsId] = (await _context.ProductsTable.FindAsync(item.productsId))?.productsQuantity ?? 0;
            }

            // Prepare the view model
            var viewModel = new ProductsCartViewModel
            {
                ProductsCart = new ProductsCart { Items = productsCartItem }, // Use the cartItems variable here directly
                ImageUrls = imageUrls,
                MaxQuantities = maxQuantities
            };

            return View(viewModel);
        }

        private async Task<Dictionary<string, string>> GetImageUrlsForCartItems(IEnumerable<ProductsCartItem> items)
        {
            // Connect to the AWS account
            List<string> keys = getKeys();
            var s3Client = new AmazonS3Client(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);
            var imageUrls = new Dictionary<string, string>();

            foreach (var item in items)
            {
                string key = item.productsImage?.Trim();
                if (!string.IsNullOrEmpty(key))
                {
                    // Construct the image URL
                    string imageUrl = $"https://{bucketname}.s3.amazonaws.com/{key}";
                    imageUrls[key] = imageUrl;
                }
            }

            return imageUrls;
        }

        private async Task<ProductsCart> GetOrCreateProductsCartForUser()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            string userId = user.Id; // Keep it as string if your IDs are strings

            var productsCart = await _context.ProductsCartTable
                                     .Include(c => c.Items)
                                     .FirstOrDefaultAsync(c => c.userId == userId);

            if (productsCart == null)
            {
                productsCart = new ProductsCart { userId = userId };
                _context.ProductsCartTable.Add(productsCart);
                await _context.SaveChangesAsync();
            }

            return productsCart;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToProductsCart(int productsId, int quantity)
        {
            var products = await _context.ProductsTable.FirstOrDefaultAsync(p => p.productsId == productsId);

            if (products == null)
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

            var productsCart = await GetOrCreateProductsCartForUser();
            var productsCartItem = productsCart.Items.FirstOrDefault(ci => ci.productsId == productsId);

            if (productsCartItem != null)
            {
                // If the item already exists in the cart, just update the quantity
                productsCartItem.productsQuantity += quantity;
            }
            else
            {
                // Otherwise, create a new CartItem and add it to the Cart
                productsCartItem = new ProductsCartItem
                {
                    productsId = products.productsId,
                    productsName = products.productsName,
                    productsQuantity = quantity,
                    productsPrice = products.productsPrice,
                    productsImage = products.productsImage, // Adjust the default image path as necessary
                    ProductsCart = productsCart
                };

                productsCart.Items.Add(productsCartItem);
            }

            await _context.SaveChangesAsync();

            TempData["ItemAdded"] = true;

            return RedirectToAction("BrowseProducts", "Products"); // Redirect to the cart index view
        }

        // POST: ProductsCart/UpdateQuantity/5
        // POST: ProductsCart/UpdateQuantity/5
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productsCartItemId, int quantity)
        {
            var productsCartItem = await _context.ProductsCartItemTable
                                         .Include(ci => ci.Products) // Make sure you include the navigation property
                                         .FirstOrDefaultAsync(ci => ci.productsCartItemId == productsCartItemId);

            if (productsCartItem == null)
            {
                return NotFound(); // or handle as appropriate if the cart item doesn't exist
            }

            // Check if there is associated goods and the quantity is valid
            if (productsCartItem.Products != null && productsCartItem.Products.productsQuantity > 0)
            {
                int updatedQuantity = Math.Min(quantity, productsCartItem.Products.productsQuantity);
                productsCartItem.productsQuantity = updatedQuantity; // update the quantity
                await _context.SaveChangesAsync();

                // If quantity had to be adjusted, notify the user
                if (updatedQuantity < quantity)
                {
                    TempData["QuantityAdjusted"] = $"The quantity for {productsCartItem.Products.productsName} has been adjusted to {updatedQuantity} due to stock limits.";
                }
            }
            else
            {
                // If the goods are not available, remove the cart item and notify the user
                _context.ProductsCartItemTable.Remove(productsCartItem);
                await _context.SaveChangesAsync();
                TempData["RemovedItems"] = $"The item {productsCartItem.Products?.productsName ?? "with ID: " + productsCartItemId} was removed from your cart as it is no longer available.";
            }

            return RedirectToAction(nameof(Index));
        }


        // POST: Cart/RemoveFromCart/5
        [HttpPost]
        public IActionResult RemoveFromCart(int productsCartItemId)
        {
            var productscartItem = _context.ProductsCartItemTable.Find(productsCartItemId);
            if (productscartItem != null)
            {
                _context.ProductsCartItemTable.Remove(productscartItem);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckout(ProductsCheckoutViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

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


            // Start a transaction in case anything goes wrong you can rollback
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Create and populate the order from the model
                    var productsOrder = new ProductsOrder
                    {
                        Email = user.Email,
                        ProductsOrderDate = DateTime.Now,
                        Contact = model.PhoneNumber,
                        Address = model.Address,
                        PaymentMethod = "Credit Card/Debit Card",
                        PaymentStatus = "Completed",
                        OrderStatus = "In Progress"
                    };

                    // Get the cart items
                    var productsCartItems = await _context.ProductsCartItemTable
                                                  .Where(c => c.ProductsCart.userId == user.Id)
                                                  .ToListAsync();

                    // Convert cart items to order items
                    foreach (var cartItem in productsCartItems)
                    {
                        var productsOrderItem = new ProductsOrderItem
                        {
                            productsId = cartItem.productsId,
                            productsName = cartItem.productsName,
                            productsQuantity = cartItem.productsQuantity,
                            productsPrice = cartItem.productsPrice,
                            productsImage = cartItem.productsImage
                        };

                        productsOrder.ProductsOrderItems.Add(productsOrderItem);
                    }

					// Deduct the purchased quantities from ProductsTable
					foreach (var cartItem in productsCartItems)
					{
						var productsItem = await _context.ProductsTable.FirstOrDefaultAsync(g => g.productsId == cartItem.productsId);
						if (productsItem != null && productsItem.productsQuantity >= cartItem.productsQuantity)
						{
							productsItem.productsQuantity -= cartItem.productsQuantity; // Deduct the quantity
							if (productsItem.productsQuantity == 0)
							{
								productsItem.productsStatus = "Out of Stocks"; // Update the status to Unavailable
							}
						}
						else
						{
							// Handle the case where there isn't enough stock
							// You might want to return a user-friendly error message or handle this scenario differently
							transaction.Rollback();
							return View("NotEnoughStock", cartItem);
						}
					}

					// Save changes for GoodsTable quantities
					await _context.SaveChangesAsync();

					// Save the order to the database
					_context.ProductsOrderTable.Add(productsOrder);
                    await _context.SaveChangesAsync();

                    // Clear the cart after order placement
                    _context.ProductsCartItemTable.RemoveRange(productsCartItems);
                    await _context.SaveChangesAsync();

                    // Commit the transaction
                    transaction.Commit();

                    // Redirect to a confirmation page or similar
                    return RedirectToAction("ProductsOrderConfirmation", new { productsOrderId = productsOrder.ProductsOrderId });
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


        public async Task<IActionResult> ProductsOrderConfirmation(int ? productsOrderId)
        {
            if (productsOrderId == null)
            {
                return NotFound();
            }

            var productsOrder = await _context.ProductsOrderTable
                                      .Include(o => o.ProductsOrderItems)
                                      .FirstOrDefaultAsync(m => m.ProductsOrderId == productsOrderId);

            if (productsOrder == null)
            {
                return NotFound();
            }

            return View(productsOrder);
        }

}
}
