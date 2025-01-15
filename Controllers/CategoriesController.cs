using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.Models;
namespace QuizApp.Controllers;

[ApiController]
[Route("api/[controller]")]

public class CategoriesController : ControllerBase
{
    private readonly PostgresContext _context;

    public CategoriesController(PostgresContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.Categories.Select(c => new { c.Id, c.Name }).ToListAsync();
        return Ok(categories);
    }
    
    // POST: /api/categories
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] Category newCategory)
    {
        if (string.IsNullOrWhiteSpace(newCategory.Name))
        {
            return BadRequest("Category name cannot be empty.");
        }

        // Check if the category name already exists
        var existingCategory = await _context.Categories.SingleOrDefaultAsync(c => c.Name == newCategory.Name);
        if (existingCategory != null)
        {
            return BadRequest("Category name already exists.");
        }

        // Add the new category to the database
        _context.Categories.Add(newCategory);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Category created successfully!" });
    }
}