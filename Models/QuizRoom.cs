namespace QuizApp.Models
{
    public class QuizRoom
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string HostUsername { get; set; }
        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
        public ICollection<RoomParticipant> Participants { get; set; } = new List<RoomParticipant>();
    }

    public class RoomParticipant
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int UserId { get; set; }
        public QuizRoom Room { get; set; }
        public AppUser User { get; set; }
    }
}