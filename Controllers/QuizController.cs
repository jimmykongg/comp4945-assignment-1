using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QuizApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly PostgresContext _context;

        public QuizController(PostgresContext context)
        {
            _context = context;
        }

        // Get quizzes by category
        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetQuizzesByCategory(int categoryId)
        {
            var quizzes = await _context.Quizzes
                .Where(q => q.CategoryId == categoryId)
                .OrderBy(q => q.Id)
                .Include(q => q.Answers)
                .ToListAsync();

            if (!quizzes.Any())
            {
                return NotFound(new { message = "No quizzes found for this category." });
            }

            var quizData = quizzes.Select(q => new
            {
                q.Id,
                q.Description,
                Answers = q.Answers.Select(a => new
                {
                    a.Id,
                    a.Description,
                    a.RightAnswer
                }).ToList()
            });

            return Ok(quizData);
        }

        // Add a new quiz with answers
        [HttpPost("add/{categoryId}")]
        public async Task<IActionResult> AddQuiz(int categoryId, [FromBody] Quiz request)
        {
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == categoryId);
            if (!categoryExists)
            {
                return NotFound(new { message = "Category not found." });
            }

            var newQuiz = new Quiz
            {
                Description = request.Description,
                CategoryId = categoryId
            };

            _context.Quizzes.Add(newQuiz);
            await _context.SaveChangesAsync(); // Save quiz and get generated QuizId

            foreach (var answer in request.Answers)
            {
                answer.QuizId = newQuiz.Id;
            }

            _context.Answers.AddRange(request.Answers);
            await _context.SaveChangesAsync(); // Save answers in one go

            return Ok(new { message = "Quiz added successfully." });
        }

        // Delete a quiz (including answers)
        [HttpDelete("delete/{quizId}")]
        public async Task<IActionResult> DeleteQuiz(int quizId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
            {
                return NotFound(new { message = "Quiz not found." });
            }

            _context.Answers.RemoveRange(quiz.Answers); // Remove related answers
            _context.Quizzes.Remove(quiz); // Remove quiz

            await _context.SaveChangesAsync();

            return Ok(new { message = "Quiz deleted successfully." });
        }
    }
}
