using System.Text.Json.Serialization;

namespace QuizApp.Models;

public partial class Answer
{
    public int Id { get; set; }

    public int QuizId { get; set; }

    public string Description { get; set; } = null!;

    public bool RightAnswer { get; set; }
    
    public virtual Quiz? Quiz { get; set; }
}
