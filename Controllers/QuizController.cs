using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using QuizApp.Models;

namespace QuizApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuizController : ControllerBase
{
    private readonly PostgresContext _context;

    public QuizController(PostgresContext context)
    {
        _context = context;
    }

    // Example: api/quiz/category/1 returns all questions and answers in category 1
    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetQuestionsByCategory(int categoryId)
    {
        var quizzes = await _context.Quizzes
            .Where(q => q.CategoryId == categoryId)
            .OrderBy(q => q.Id)
            .Include(q => q.Answers)
            .ToListAsync();

        if (quizzes == null || quizzes.Count == 0)
        {
            return NotFound(new { message = "No quizzes found for the given category." });
        }

        return Ok(quizzes.Select(quiz => new
        {
            Description = quiz.Description,
            Answers = quiz.Answers.Select(a => new
            {
                AnswerId = a.Id,
                Description = a.Description,
                IsCorrect = a.RightAnswer
            })
        }));
    }
}