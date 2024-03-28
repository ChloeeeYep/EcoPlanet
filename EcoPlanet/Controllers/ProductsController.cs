using Microsoft.AspNetCore.Mvc;
using EcoPlanet.Models;
using EcoPlanet.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Build.Execution;
using Amazon; //for linking your AWS account
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration; //appsettings.json section
using System.IO; // input output
using Microsoft.AspNetCore.Http;
using NuGet.Packaging.Signing;
using System.Drawing;

namespace EcoPlanet.Controllers
{
    public class ProductsController : Controller
    {
        private readonly EcoPlanetContext _context;
        private const string bucketname = "ecoplanet";


        public ProductsController(EcoPlanetContext context)
        {
            _context = context;
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
            //1.connect to the AWS account
            List<string> keys = getKeys();
            AmazonS3Client agent = new AmazonS3Client(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);

            var products = await _context.ProductsTable.ToListAsync();

            //2. create empty lists that can store the retrieved images from S3
            List<S3Object> imagelist = new List<S3Object>();

            //3.read image by image and store to the lists
            string ? nextToken = null;
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

            // Create the ViewModel and populate it with tarshpedia and image data
            var viewModel = new ProductsViewModel
            {
                ProductsList = products,
                ImageList = imagelist
            };

            return View(viewModel);
        }



        public IActionResult AddProducts()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProducts(Products products, List<IFormFile> productsImage)
        {
            // 1. Connect to the AWS account and retrieve keys
            List<string> keys = getKeys();
            AmazonS3Client agent = new AmazonS3Client(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);

            // 2. Upload the image file(s) to S3 and save the filename(s) to the trashpediaImage property
            foreach (var image in productsImage)
            {
                // Check for valid image file
                if (image.Length <= 0)
                {
                    return BadRequest("Image of " + image.FileName + " is an empty file. Not able to upload.");
                }
                else if (image.Length > 2097152) // 2MB limit
                {
                    return BadRequest("Image of " + image.FileName + " is more than 2MB. Not able to upload.");
                }
                else if (image.ContentType.ToLower() != "image/png" && image.ContentType.ToLower() != "image/jpeg")
                {
                    return BadRequest("Image of " + image.FileName + " is an invalid file type. Not able to upload.");
                }

                // Upload image to S3
                try
                {
                    PutObjectRequest request = new PutObjectRequest
                    {
                        InputStream = image.OpenReadStream(),
                        BucketName = bucketname,
                        Key = "ProductsImage/" + image.FileName, // Specify the S3 key where the image will be stored
                        CannedACL = S3CannedACL.PublicRead // Set the ACL to allow public read access
                    };

                    await agent.PutObjectAsync(request);

                    // Assign the filename (key) to theproductsImage property
                    products.productsImage = "ProductsImage/" + image.FileName; // Store the S3 key as the trashpediaImage property
                }
                catch (AmazonS3Exception ex)
                {
                    return BadRequest("Image of " + image.FileName + " is facing technical issues in AWS Side. Error:  " + ex.Message);
                }
                catch (Exception ex)
                {
                    return BadRequest("Image of " + image.FileName + " is facing technical issues. Error: " + ex.Message);
                }
            }

            // Save the products object to the database
            if (ModelState.IsValid)
            {
                _context.ProductsTable.Add(products);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(products);
        }

        public async Task<IActionResult> EditProducts(int ? productsId)
        {
            if (productsId == null)
            {
                return NotFound();
            }
            var products = await _context.ProductsTable.FindAsync(productsId);

            if (products == null)
            {
                return BadRequest(productsId + " is not found in the table");
            }
            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProducts(Products products, List<IFormFile> productsImage)
        {
            foreach (var image in productsImage)
            {
                try
                {
                    List<string> keys = getKeys();
                    AmazonS3Client agent = new AmazonS3Client(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);

                    //upload to S3
                    PutObjectRequest request = new PutObjectRequest
                    {
                        InputStream = image.OpenReadStream(),
                        BucketName = bucketname,
                        Key = "ProductsImage/" + image.FileName, // Specify the S3 key where the image will be stored
                        CannedACL = S3CannedACL.PublicRead // Set the ACL to allow public read access
                    };

                    await agent.PutObjectAsync(request);
                }
                catch (Exception ex)
                {
                    return BadRequest("Error: " + ex.Message);
                }

                // Assign the filename (key) to the goodsImage property
                products.productsImage = "ProductsImage/" + image.FileName;
            }

            if (ModelState.IsValid)
            {
                _context.ProductsTable.Update(products);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Products");
            }
            else
            {
                return View("EditProducts", products);
            }
        }

        public async Task<IActionResult> DeleteProducts(int ? productsId)
        {
            try
            {
                if (productsId == null)
                {
                    return NotFound();
                }

                var products = await _context.ProductsTable.FindAsync(productsId);

                if (products == null)
                {
                    return NotFound();
                }

                List<string> keys = getKeys(); // Retrieve AWS keys

                AmazonS3Client agent = new AmazonS3Client(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);

                // Construct the S3 object key
                string s3ObjectKey = products.productsImage;

                // Check if the object exists in the S3 bucket before attempting deletion
                DeleteObjectRequest deleteRequest = new DeleteObjectRequest
                {
                    BucketName = bucketname,
                    Key = s3ObjectKey,
                };


                await agent.DeleteObjectAsync(deleteRequest);

                // Remove the Trashpedia record from the database
                _context.ProductsTable.Remove(products);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Products");
            }
            catch (Exception ex)
            {
                return BadRequest("Error deleting products: " + ex.Message);
            }
        }

    }
}
