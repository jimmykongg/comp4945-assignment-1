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

        if (quizzes == null)
        {
            return NotFound(new { message = "No quizzes found for the given category." });
        }

        return Ok(quizzes.Select(quiz => new
        {
            Description = quiz.Description,
            Id = quiz.Id,
            Answers = quiz.Answers.Select(a => new
            {
                AnswerId = a.Id,
                Description = a.Description,
                IsCorrect = a.RightAnswer
            })
        }));
    }
    
    [HttpPost("add/{categoryId}")]
    public async Task<IActionResult> AddQuizWithAnswers(int categoryId, [FromBody] Quiz request)
    {
        // Check if the category exists
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == categoryId);
        if (!categoryExists)
        {
            return NotFound(new { message = "Category not found." });
        }


        // Step 1: Create the quiz
        var newQuiz = new Quiz
        {
            Description = request.Description,
            CategoryId = categoryId
        };

        _context.Quizzes.Add(newQuiz);
        await _context.SaveChangesAsync();  // Save quiz and get the generated QuizId

        // Step 2: Assign QuizId to each answer and add answers
        foreach (var answer in request.Answers)
        {
            answer.QuizId = newQuiz.Id;  // Assign the newly created QuizId
        }

        _context.Answers.AddRange(request.Answers);
        await _context.SaveChangesAsync();  // Save answers in one go

        return Ok(new { message = "Quiz and answers added successfully!" });
    }
    
    // delete a quiz and answers
    [HttpDelete("delete/{quizId}")]
    public async Task<IActionResult> DeleteQuiz(int quizId)
    {
        // Retrieve the quiz with its answers
        var quiz = await _context.Quizzes
            .Include(q => q.Answers)  // Include related answers
            .FirstOrDefaultAsync(q => q.Id == quizId);

        if (quiz == null)
        {
            return NotFound(new { message = "Quiz not found." });
        }

        // Remove all related answers
        _context.Answers.RemoveRange(quiz.Answers);

        // Remove the quiz
        _context.Quizzes.Remove(quiz);
    
        await _context.SaveChangesAsync();  // Save changes to the database

        return Ok(new { message = "Quiz deleted successfully." });
    }
    
    [HttpPut("update/{quizId}")]
    public async Task<IActionResult> UpdateQuiz(int quizId, [FromBody] Quiz request)
    {
        // Find the quiz and include related answers
        var existingQuiz = await _context.Quizzes
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == quizId);

        if (existingQuiz == null)
        {
            return NotFound(new { message = "Quiz not found." });
        }
        
        // Update quiz description
        existingQuiz.Description = request.Description;

        // Clear the old answers and add updated answers
        _context.Answers.RemoveRange(existingQuiz.Answers);
        foreach (var answer in request.Answers)
        {
            _context.Answers.Add(new Answer
            {
                QuizId = quizId,
                Description = answer.Description,
                RightAnswer = answer.RightAnswer
            });
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Quiz updated successfully!" });
    }



}