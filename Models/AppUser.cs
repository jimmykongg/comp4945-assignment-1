using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuizApp.Models;

public partial class AppUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public DateTime? DateOfBirth { get; set; } = DateTime.Today;
    
    public byte[]? Photo { get; set; }

    [RegularExpression(@"^[a-zA-Z'\s]{1,20}$", 
        ErrorMessage = "Letters only, between 1 and 20 characters long.")]
    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Role { get; set; }
}
