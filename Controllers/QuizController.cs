using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using QuizApp.Models;

namespace QuizApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly PostgresContext _context;
        private readonly IWebHostEnvironment _env;

        public QuizController(PostgresContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private void DeleteFile(string relativePath)
        {
            try
            {
                var fullPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting file {relativePath}: {ex.Message}");
            }
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetQuestionsByCategory(int categoryId)
        {
            var quizzes = await _context.Quizzes
                .Where(q => q.CategoryId == categoryId)
                .OrderBy(q => q.Id)
                .Include(q => q.Answers)
                .Include(q => q.Media)
                .ToListAsync();

            if (!quizzes.Any())
            {
                return NotFound(new { message = "No quizzes found for the given category." });
            }

            var result = quizzes.Select(quiz => new
            {
                quiz.Id,
                quiz.Description,
                Media = quiz.Media != null ? new
                {
                    quiz.Media.MediaType,
                    quiz.Media.FilePath
                } : null,
                Answers = quiz.Answers.Select(a => new
                {
                    a.Id,
                    a.Description,
                    a.RightAnswer
                })
            });

            return Ok(result);
        }

[HttpPost("add/{categoryId}")]
public async Task<IActionResult> AddQuizWithAnswers(
    int categoryId,
    [FromForm] string Description,
    [FromForm] string Answers, // Serialized JSON string
    [FromForm] IFormFile? MediaFile,
    [FromForm] string? MediaType,
    [FromForm] string? YouTubeLink)
{
    var categoryExists = await _context.Categories.AnyAsync(c => c.Id == categoryId);
    if (!categoryExists)
    {
        return NotFound(new { message = "Category not found." });
    }

    int? mediaId = null;

    // Handle media file or YouTube link
    if (MediaType == "video" && !string.IsNullOrEmpty(YouTubeLink))
    {
        var medium = new Medium
        {
            MediaType = MediaType,
            FilePath = YouTubeLink
        };

        _context.Media.Add(medium);
        await _context.SaveChangesAsync();
        mediaId = medium.Id;
    }
    else if (MediaFile != null && !string.IsNullOrEmpty(MediaType))
    {
        var filePath = await SaveMediaFile(MediaFile, MediaType);
        var medium = new Medium
        {
            MediaType = MediaType,
            FilePath = filePath
        };

        _context.Media.Add(medium);
        await _context.SaveChangesAsync();
        mediaId = medium.Id;
    }

    // Create a new quiz
    var newQuiz = new Quiz
    {
        Description = Description,
        CategoryId = categoryId,
        MediaId = mediaId
    };

    _context.Quizzes.Add(newQuiz);
    await _context.SaveChangesAsync();

    // Deserialize and add answers
    try
    {
        var answers = System.Text.Json.JsonSerializer.Deserialize<List<Answer>>(Answers);
        if (answers != null)
        {
            foreach (var answer in answers)
            {
                answer.QuizId = newQuiz.Id;
            }

            _context.Answers.AddRange(answers);
            await _context.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        return BadRequest(new { message = $"Failed to parse answers: {ex.Message}" });
    }

    return Ok(new { message = "Quiz and answers added successfully!", QuizId = newQuiz.Id });
}


        [HttpDelete("delete/{quizId}")]
        public async Task<IActionResult> DeleteQuiz(int quizId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Answers)
                .Include(q => q.Media)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
            {
                return NotFound(new { message = "Quiz not found." });
            }

            if (quiz.Media != null)
            {
                DeleteFile(quiz.Media.FilePath);
                _context.Media.Remove(quiz.Media);
            }

            _context.Answers.RemoveRange(quiz.Answers);
            _context.Quizzes.Remove(quiz);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Quiz deleted successfully." });
        }

        [HttpPut("update/{quizId}")]
        public async Task<IActionResult> UpdateQuiz(
            int quizId,
            [FromForm] string description,
            [FromForm] string answers, // Serialized JSON string
            [FromForm] IFormFile? mediaFile,
            [FromForm] string? mediaType,
            [FromForm] string? youTubeLink)
        {
            try
            {
                var existingQuiz = await _context.Quizzes
                    .Include(q => q.Answers)
                    .Include(q => q.Media)
                    .FirstOrDefaultAsync(q => q.Id == quizId);

                if (existingQuiz == null)
                {
                    return NotFound(new { message = "Quiz not found." });
                }

                // Update quiz description
                existingQuiz.Description = description;

                // Update media
                if (mediaType == "video" && !string.IsNullOrEmpty(youTubeLink))
                {
                    // Handle YouTube video
                    if (existingQuiz.MediaId.HasValue)
                    {
                        var media = await _context.Media.FindAsync(existingQuiz.MediaId);
                        if (media != null)
                        {
                            media.MediaType = "video";
                            media.FilePath = youTubeLink;
                            _context.Media.Update(media);
                        }
                    }
                    else
                    {
                        var newMedia = new Medium
                        {
                            MediaType = "video",
                            FilePath = youTubeLink
                        };
                        _context.Media.Add(newMedia);
                        await _context.SaveChangesAsync();
                        existingQuiz.MediaId = newMedia.Id;
                    }
                }
                else if (mediaFile != null && !string.IsNullOrEmpty(mediaType))
                {
                    if (existingQuiz.MediaId.HasValue)
                    {
                        var oldMedia = await _context.Media.FindAsync(existingQuiz.MediaId);
                        if (oldMedia != null)
                        {
                            DeleteFile(oldMedia.FilePath);
                            _context.Media.Remove(oldMedia);
                            existingQuiz.MediaId = null;
                        }
                    }

                    var filePath = await SaveMediaFile(mediaFile, mediaType);
                    var newMedia = new Medium
                    {
                        MediaType = mediaType,
                        FilePath = filePath
                    };

                    _context.Media.Add(newMedia);
                    await _context.SaveChangesAsync();
                    existingQuiz.MediaId = newMedia.Id;
                }

                // Update answers
                if (!string.IsNullOrEmpty(answers))
                {
                    var updatedAnswers = System.Text.Json.JsonSerializer.Deserialize<List<Answer>>(answers);
                    if (updatedAnswers != null)
                    {
                        _context.Answers.RemoveRange(existingQuiz.Answers);
                        foreach (var answer in updatedAnswers)
                        {
                            _context.Answers.Add(new Answer
                            {
                                QuizId = quizId,
                                Description = answer.Description,
                                RightAnswer = answer.RightAnswer
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Quiz updated successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Failed to update quiz: {ex.Message}" });
            }
        }

        private async Task<string> SaveMediaFile(IFormFile mediaFile, string mediaType)
        {
            var uploadDir = Path.Combine(_env.WebRootPath, "images");
            Directory.CreateDirectory(uploadDir);
            var fileName = Path.GetFileName(mediaFile.FileName);
            var filePath = Path.Combine(uploadDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await mediaFile.CopyToAsync(stream);
            }

            return $"/images/{fileName}";
        }
    }
}
