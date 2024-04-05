using Microsoft.AspNetCore.Mvc;
using EcoPlanet.Models;
using EcoPlanet.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Amazon.S3.Model;
using Amazon.S3;
using Microsoft.Build.Execution;
using Amazon; //for linking your AWS account
using Microsoft.Extensions.Configuration; //appsettings.json section
using System.IO; // input output
using Microsoft.AspNetCore.Http;
using NuGet.Packaging.Signing;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;

namespace EcoPlanet.Controllers
{
    public class QuizController : Controller
    {

        private readonly EcoPlanetContext _context;
        private const string bucketname = "ecoplanet";

        public QuizController(EcoPlanetContext context)
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
            List<Quiz> quizzes = await _context.QuizTable.ToListAsync();
            return View(quizzes);
        }

        public IActionResult AddData()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddData(Quiz quiz)
        {        
            // Save the quiz object to the database
            if (ModelState.IsValid)
            {
                _context.QuizTable.Add(quiz);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(quiz);
        }

        public async Task<IActionResult> EditData(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            var quiz = await _context.QuizTable.FindAsync(Id);

            if (quiz == null)
            {
                return BadRequest(Id + " is not found in the table!");
            }

            return View(quiz);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateData(Quiz quiz)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.QuizTable.Update(quiz);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Quiz");
                }
                return View("EditData", quiz);
            }
            catch(Exception ex)
            {
                return BadRequest("Error: " + ex.Message);
            }           
        }

        public async Task<IActionResult> DeleteData(int? Id) 
        { 
            if (Id == null) 
            {
                return NotFound(); 
            } 
            
            var quiz = await _context.QuizTable.FindAsync(Id);

            if (quiz == null) 
            { 
                return BadRequest(Id + " is not found in the list!");
            }
            _context.QuizTable.Remove(quiz); 

            await _context.SaveChangesAsync(); return RedirectToAction("Index", "Quiz"); 
        }

        public async Task<ActionResult> ShowQuiz(int? index)
        {
            var quizzes = await _context.QuizTable.ToListAsync();
            var viewModel = new QuizViewModel
            {
                Questions = quizzes,
                UserAnswers = new List<string>(new string[quizzes.Count]),
                CurrentQuestionIndex = index ?? 0,
                IsFinished = (index.HasValue && index >= quizzes.Count)
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> SubmitQuiz([FromBody] List<string> answers)
        {
            var questions = await _context.QuizTable.ToListAsync();
            int score = 0;

            for (int i = 0; i < questions.Count; i++)
            {
                if (!string.IsNullOrEmpty(answers[i]) && answers[i].Equals(questions[i].Correct?.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    score++;
                }
            }

            // Store the total number of questions in the TempData to pass to the next request
            TempData["TotalQuestions"] = questions.Count;

            return Json(new { score });
        }



        public ActionResult QuizResults(int score)
        {
            // Retrieve the total number of questions from TempData
            var totalQuestions = TempData["TotalQuestions"] ?? 0;
            ViewBag.Score = score;
            ViewBag.TotalQuestions = totalQuestions; 
            return View();
        }

        private async Task<string> GetImageUrlAsync()
        {
            List<string> keys = getKeys();
            AmazonS3Client agent = new AmazonS3Client(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);

            string imageKey = "Design/tips.png";

            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketname,
                Key = imageKey,
                Expires = DateTime.Now.AddMinutes(10)
            };

            return agent.GetPreSignedURL(request);
        }

        public async Task<IActionResult> IntroPage()
        {
            var imageUrl = await GetImageUrlAsync();

            ViewBag.ImageUrl = imageUrl;
            return View();
        }


    }
}
