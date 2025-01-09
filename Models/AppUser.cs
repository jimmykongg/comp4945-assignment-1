using System;
using System.Collections.Generic;

namespace QuizApp.Models;

public partial class AppUser
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Role { get; set; }
}
