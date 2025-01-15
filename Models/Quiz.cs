using System.Text.Json.Serialization;

namespace QuizApp.Models;

public partial class Quiz
{
    public int Id { get; set; }

    public string Description { get; set; } = null!;

    public int? MediaId { get; set; }

    public int CategoryId { get; set; }

    
    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
    
    public virtual Category? Category { get; set; }
    
    public virtual Medium? Media { get; set; }
}
