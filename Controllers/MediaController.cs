using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using QuizApp.Models;

namespace QuizApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediaController : ControllerBase
{
    private readonly PostgresContext _context;
    private readonly IWebHostEnvironment _env;

    public MediaController(PostgresContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // GET: api/media/{mediaId}
    [HttpGet("{mediaId}")]
    public async Task<IActionResult> GetMedia(int mediaId)
    {
        var media = await _context.Media.FindAsync(mediaId);

        if (media == null)
        {
            return NotFound(new { message = "Media content not found." });
        }

        return Ok(new
        {
            MediaType = media.MediaType,
            FilePath = media.FilePath
        });
    }

    // POST: api/media/upload
    [HttpPost("upload")]
    public async Task<IActionResult> UploadMedia([FromForm] IFormFile file, [FromForm] string mediaType)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded." });
        }

        string uploadDir = Path.Combine(_env.WebRootPath, "images");
        Directory.CreateDirectory(uploadDir); // Ensure the directory exists
        string fileName = Path.GetFileName(file.FileName);
        string filePath = Path.Combine(uploadDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var media = new Medium
        {
            MediaType = mediaType,
            FilePath = $"/images/{fileName}"
        };

        _context.Media.Add(media);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            MediaId = media.Id,
            MediaType = media.MediaType,
            FilePath = media.FilePath
        });
    }
}
