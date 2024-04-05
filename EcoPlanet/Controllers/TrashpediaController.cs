using Microsoft.AspNetCore.Mvc;
using EcoPlanet.Models;
using EcoPlanet.Data;
using Microsoft.EntityFrameworkCore;
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
    public class TrashpediaController : Controller
    {
        private readonly EcoPlanetContext _context;
        private const string bucketname = "ecoplanet";

        public TrashpediaController(EcoPlanetContext context)
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

            var trashpedia = await _context.TrashpediaTable.ToListAsync();
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

            // Create the ViewModel and populate it with tarshpedia and image data
            var viewModel = new TrashpediaViewModel
            {
                TrashpediaList = trashpedia,
                ImageList = imagelist
            };

            return View(viewModel);
        }


        public IActionResult AddData()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddData(Trashpedia trashpedia, List<IFormFile> Images)
        {
            // 1. Connect to the AWS account and retrieve keys
            List<string> keys = getKeys();
            AmazonS3Client agent = new AmazonS3Client(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);

            // 2. Upload the image file(s) to S3 and save the filename(s) to the trashpediaImage property
            foreach (var image in Images)
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
                        Key = "Images/" + image.FileName, // Specify the S3 key where the image will be stored
                        CannedACL = S3CannedACL.PublicRead // Set the ACL to allow public read access
                    };

                    await agent.PutObjectAsync(request);

                    // Assign the filename (key) to the trashpediaImage property
                    trashpedia.Images = "Images/" + image.FileName; // Store the S3 key as the trashpediaImage property
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

            // Save the trashpedia object to the database
            if (ModelState.IsValid)
            {
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
        public async Task<IActionResult> UpdateData(Trashpedia trashpedia, List<IFormFile> Images)
        {
            // Only update the image if a new image is uploaded.
            if (Images != null && Images.Count > 0)
            {
                foreach (var image in Images)
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
                            Key = "Images/" + image.FileName, // Specify the S3 key where the image will be stored
                            CannedACL = S3CannedACL.PublicRead // Set the ACL to allow public read access
                        };

                        await agent.PutObjectAsync(request);
                        trashpedia.Images = "Images/" + image.FileName;
                    }
                    catch (Exception ex)
                    {
                        return BadRequest("Error: " + ex.Message);
                    }
                }
            }
            // If no new image is provided, keep the old image path.
            else
            {
                var existingTrashpedia = await _context.TrashpediaTable.AsNoTracking().FirstOrDefaultAsync(t => t.Id == trashpedia.Id);
                if (existingTrashpedia != null)
                {
                    trashpedia.Images = existingTrashpedia.Images;
                }
            }

            if (ModelState.IsValid)
            {
                trashpedia.CreatedAt = DateTime.Now;
                _context.TrashpediaTable.Update(trashpedia);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Trashpedia");
            }
            else
            {
                return View("EditData", trashpedia);
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

                List<string> keys = getKeys(); // Retrieve AWS keys

                AmazonS3Client agent = new AmazonS3Client(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);

                // Construct the S3 object key
                string s3ObjectKey = trashpedia.Images;

                // Check if the object exists in the S3 bucket before attempting deletion
                DeleteObjectRequest deleteRequest = new DeleteObjectRequest
                {
                    BucketName = bucketname,
                    Key = s3ObjectKey,
                };


                await agent.DeleteObjectAsync(deleteRequest);

                // Remove the Trashpedia record from the database
                _context.TrashpediaTable.Remove(trashpedia);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Trashpedia");
            }
            catch (Exception ex)
            {
                return BadRequest("Error deleting trashpedia: " + ex.Message);
            }
        }

        public async Task<IActionResult> ShowTrashpedia()
        {
            //1.connect to the AWS account
            List<string> keys = getKeys();
            AmazonS3Client agent = new AmazonS3Client(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);

            var trashpedia = await _context.TrashpediaTable.ToListAsync();
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

            // Create the ViewModel and populate it with tarshpedia and image data
            var viewModel = new TrashpediaViewModel
            {
                TrashpediaList = trashpedia,
                ImageList = imagelist
            };

            return View(viewModel);
        }

        // Add a parameter to your method to accept the id
        public async Task<IActionResult> ShowTrashpediaDetails(int id)
        {
            // Connect to the AWS account
            List<string> keys = getKeys();
            AmazonS3Client agent = new AmazonS3Client(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);

            // Fetch the specific Trashpedia item using the provided id
            var trashpediaItem = await _context.TrashpediaTable.FirstOrDefaultAsync(t => t.Id == id);
            if (trashpediaItem == null)
            {
                // Handle the case where the item is not found
                return NotFound();
            }

            // Assume that 'Images' is the property that holds the key of the image in S3
            string imageKey = trashpediaItem.Images;
            string imageUrl = $"https://ecoplanet.s3.amazonaws.com/{Uri.EscapeDataString(imageKey)}";

            // Create a ViewModel for the details view
            var viewModel = new Trashpedia
            {
                Name = trashpediaItem.Name,
                Materials = trashpediaItem.Materials,
                Actions = trashpediaItem.Actions,
                Alternatives = trashpediaItem.Alternatives,
                Images = imageUrl
            };

            return View(viewModel);
        }


    }
}