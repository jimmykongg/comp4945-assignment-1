using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QuizApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizRoomController : ControllerBase
    {
        private readonly PostgresContext _context;

        public QuizRoomController(PostgresContext context)
        {
            _context = context;
        }

        // Get a list of quiz rooms
        [HttpGet]
        public async Task<IActionResult> GetQuizRooms()
        {
            var rooms = await _context.QuizRooms
                .Include(r => r.Participants)
                .Include(r => r.Quizzes)
                .ToListAsync();

            return Ok(rooms);
        }

        // Get room details including participants and quizzes
        [HttpGet("{roomId}")]
        public async Task<IActionResult> GetQuizRoomById(int roomId)
        {
            var room = await _context.QuizRooms
                .Include(r => r.Participants)
                .Include(r => r.Quizzes)
                .FirstOrDefaultAsync(r => r.Id == roomId);

            if (room == null)
            {
                return NotFound(new { message = "Room not found." });
            }

            return Ok(room);
        }

        // Create a new quiz room
        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] QuizRoom request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Room name cannot be empty.");
            }

            var room = new QuizRoom
            {
                Name = request.Name,
                HostUsername = request.HostUsername
            };

            _context.QuizRooms.Add(room);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Room created successfully.", roomId = room.Id });
        }

        // Add a participant to a quiz room
        [HttpPost("join/{roomId}")]
        public async Task<IActionResult> JoinRoom(int roomId, [FromBody] RoomParticipant participant)
        {
            var room = await _context.QuizRooms
                .Include(r => r.Participants)
                .FirstOrDefaultAsync(r => r.Id == roomId);

            if (room == null)
            {
                return NotFound(new { message = "Room not found." });
            }

            // Ensure the participant is not already in the room
            if (room.Participants.Any(p => p.UserId == participant.UserId))
            {
                return BadRequest("User is already a participant.");
            }

            room.Participants.Add(participant);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Participant added successfully." });
        }

        // Remove a participant from a quiz room
        [HttpDelete("leave/{roomId}/{userId}")]
        public async Task<IActionResult> LeaveRoom(int roomId, int userId)
        {
            var room = await _context.QuizRooms
                .Include(r => r.Participants)
                .FirstOrDefaultAsync(r => r.Id == roomId);

            if (room == null)
            {
                return NotFound(new { message = "Room not found." });
            }

            var participant = room.Participants.FirstOrDefault(p => p.UserId == userId);
            if (participant == null)
            {
                return NotFound(new { message = "Participant not found." });
            }

            room.Participants.Remove(participant);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Participant removed successfully." });
        }
    }
}
