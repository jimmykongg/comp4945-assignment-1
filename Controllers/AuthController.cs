using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using QuizApp.Models;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace QuizApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly PostgresContext _context;

    public AuthController(PostgresContext context)
    {
        _context = context;
    }

    // api/auth/signup
    [HttpPost("signup")]
    public async Task<IActionResult> SignUp([FromBody] AppUser req)
    {
        if (req.Username == null || req.Password == null || req.Role == null)
        {
            return BadRequest("Username, password, and role are required.");
        }
        
        if (await _context.AppUsers.AnyAsync(u => u.Username == req.Username))
        {
            return BadRequest("Username already exists.");
        }
        
        req.Password = HashPassword(req.Password);
        _context.AppUsers.Add(req);
        await _context.SaveChangesAsync();
        
        return Ok(new { Message = "User registered successfully" });
    }

    // api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AppUser req)
    {
        Console.WriteLine("Login");
        var user = await _context.AppUsers.SingleOrDefaultAsync(u => u.Username == req.Username);

        if (user == null || !VerifyPassword(req.Password, user.Password))
        {
            return Unauthorized("Failed to login. Invalid username or password.");
        }

        var token = GenerateJwt(user);
        
        return Ok(new
        {
            Token = token,
            Message = "Login successfully.",
            Username = user.Username,
            Role = user.Role
        });
    }
    
    
    // Some helper functions
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
    
    private static bool VerifyPassword(string src, string hashed)
    {
        return HashPassword(src) == hashed;
    }
    
    private string GenerateJwt(AppUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("opjdi83nuienddh8392hd809cuid2d30?!3nefwi35i80vwioc18hd1!?"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: "QuizApp",
            audience: "QuizAppUser",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}