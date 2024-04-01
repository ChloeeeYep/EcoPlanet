using Microsoft.AspNetCore.Mvc;
using EcoPlanet.Models;
using EcoPlanet.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using EcoPlanet.Areas.Identity.Data;
using Amazon; //account purpose
using Amazon.S3; //creating s3 process
using Amazon.S3.Model; //object structure in S3
using Microsoft.Extensions.Configuration; //accessing apps.settings
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal; //for object data transmissions in network


namespace EcoPlanet.Controllers
{
    public class GoodsController : Controller
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

        public GoodsController(EcoPlanetContext context, UserManager<EcoPlanetUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User); // Gets the current logged-in user
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var email = user.Email;
            var goods = await _context.GoodsTable
                                      .Where(g => g.SellerId == email)
                                      .ToListAsync();

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

            // Create the ViewModel and populate it with goods and image data
            var viewModel = new GoodsViewModel
            {
                GoodsList = goods,
                ImageList = imagelist
            };

            return View(viewModel);
        }

        public IActionResult AddGoods()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGoods(Goods goods, List<IFormFile> goodsImage)
        {
            // 1. Connect to the AWS account and retrieve keys
            List<string> keys = getKeys();
            AmazonS3Client agent = new AmazonS3Client(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);

            // 2. Upload the image file(s) to S3 and save the filename(s) to the goodsImage property
            foreach (var image in goodsImage)
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
                        Key = "GoodsImages/" + image.FileName, // Specify the S3 key where the image will be stored
                        CannedACL = S3CannedACL.PublicRead // Set the ACL to allow public read access
                    };

                    await agent.PutObjectAsync(request);

                    // Assign the filename (key) to the goodsImage property
                    goods.goodsImage = "GoodsImages/" + image.FileName;
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

            // Save the goods object to the database
            if (ModelState.IsValid)
            {
                _context.GoodsTable.Add(goods);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(goods);
        }

        public async Task<IActionResult> EditGoods(int? goodsId)
        {
            if (goodsId == null)
            {
                return NotFound();
            }
            var goods = await _context.GoodsTable.FindAsync(goodsId);

            if (goods == null)
            {
                return BadRequest(goodsId + " is not found in the table");
            }
            return View(goods);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateGoods(Goods goods, List<IFormFile> goodsImage)
        {
            foreach (var image in goodsImage)
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
                        Key = "GoodsImages/" + image.FileName, // Specify the S3 key where the image will be stored
                        CannedACL = S3CannedACL.PublicRead // Set the ACL to allow public read access
                    };

                    await agent.PutObjectAsync(request);
                }
                catch (Exception ex)
                {
                    return BadRequest("Error: " + ex.Message);
                }

                // Assign the filename (key) to the goodsImage property
                goods.goodsImage = "GoodsImages/" + image.FileName;
            }

            if (ModelState.IsValid)
            {
                _context.GoodsTable.Update(goods);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Goods");
            }
            else
            {
                return View("EditGoods", goods);
            }
        }

        public async Task<IActionResult> DeleteGoods(int? goodsId)
        {
            try
            {
                if (goodsId == null)
                {
                    return NotFound();
                }

                var goods = await _context.GoodsTable.FindAsync(goodsId);

                if (goods == null)
                {
                    return NotFound();
                }

                List<string> keys = getKeys(); // Retrieve AWS keys

                AmazonS3Client agent = new AmazonS3Client(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);

                // Construct the S3 object key
                string s3ObjectKey = goods.goodsImage;

                // Check if the object exists in the S3 bucket before attempting deletion
                DeleteObjectRequest deleteRequest = new DeleteObjectRequest
                {
                    BucketName = bucketname,
                    Key = s3ObjectKey,
                };


                await agent.DeleteObjectAsync(deleteRequest);

                // Remove the Trashpedia record from the database
                _context.GoodsTable.Remove(goods);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Goods");
            }
            catch (Exception ex)
            {
                return BadRequest("Error deleting goods: " + ex.Message);
            }
        }

        public async Task<IActionResult> BrowseGoods()
        {
            var goods = await _context.GoodsTable.ToListAsync();

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

            // Create the ViewModel and populate it with goods and image data
            var viewModel = new GoodsViewModel
            {
                GoodsList = goods,
                ImageList = imagelist
            };

            return View(viewModel);
        }


        public async Task<IActionResult> GoodsDetails(int goodsId)
        {
            var goods = await _context.GoodsTable
                                          .FirstOrDefaultAsync(g => g.goodsId == goodsId);

            //1.connect to the AWS account
            List<string> keys = getKeys();
            AmazonS3Client agent = new AmazonS3Client(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);

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

            if (goods != null)
            {
                var viewModel = new GoodsViewModel
                {
                    GoodsList = new List<Goods> { goods },
                    ImageList = imagelist
                };

                return View(viewModel);
            }
            else
            {
                return NotFound();
            }

        }
    }
}
