using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.SignalR;
using QuizApp.Models;
using System.Text;
using QuizApp.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add PostgreSQL database context
builder.Services.AddDbContext<PostgresContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "QuizApp",
            ValidAudience = "QuizAppUser",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("opjdi83nuienddh8392hd809cuid2d30?!3nefwi35i80vwioc18hd1!?"))
        };
    });

// Add SignalR services for real-time communication (QuizRoomHub)
builder.Services.AddSignalR();

// Add CORS (if needed for your front-end)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseHttpsRedirection();

// Static files (e.g., HTML, JS) should be served from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

// Use routing for API and SignalR endpoints
app.UseRouting();

// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Enable CORS if required
app.UseCors("AllowAll");

// Map API controllers
app.MapControllers();

// Map SignalR hubs
app.MapHub<QuizRoomHub>("/quizRoomHub");

app.Run();