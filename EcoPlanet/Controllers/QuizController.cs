using Microsoft.AspNetCore.Mvc;
using EcoPlanet.Models;
using EcoPlanet.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EcoPlanet.Controllers
{
    public class QuizController : Controller
    {

        private readonly EcoPlanetContext _context;

        public QuizController(EcoPlanetContext context)
        {
            _context = context;
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


    }
}
