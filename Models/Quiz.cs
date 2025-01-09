using System;
using System.Collections.Generic;

namespace QuizApp.Models;

public partial class Quiz
{
    public int Id { get; set; }

    public string Description { get; set; } = null!;

    public int? MediaId { get; set; }

    public int CategoryId { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual Category Category { get; set; } = null!;

    public virtual Medium? Media { get; set; }
}
