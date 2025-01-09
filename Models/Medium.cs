using System;
using System.Collections.Generic;

namespace QuizApp.Models;

public partial class Medium
{
    public int Id { get; set; }

    public string? MediaType { get; set; }

    public string FilePath { get; set; } = null!;

    public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
}
