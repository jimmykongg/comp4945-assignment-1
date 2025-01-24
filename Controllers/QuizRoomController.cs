using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using QuizApp.WebSocketHandlers;

namespace QuizApp.Controllers
{
    [ApiController]
    [Route("api/quizroom")]
    public class QuizRoomController : ControllerBase
    {
        private readonly QuizRoomManager _roomManager;

        public QuizRoomController(QuizRoomManager roomManager)
        {
            _roomManager = roomManager; // Injected QuizRoomManager singleton
        }

        // Create a new quiz room (WebSocket connection for the host)
        [Route("create/{categoryId}")]
        public async Task CreateRoom(int categoryId, [FromQuery] string username)
        {
            Console.WriteLine("QuizRoomController: Creating room");
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                Console.WriteLine("QuizRoomController: WebSocket connection established.");
                var roomId = _roomManager.CreateRoom(categoryId, username, socket);
                Console.WriteLine($"QuizRoomController: Room {roomId} created successfully by {username}");
                await Task.Delay(500000);
            }
            else
            {
                HttpContext.Response.StatusCode = 400; // Bad Request
                Console.WriteLine("Failed to create room: Not a WebSocket request.");
            }
        }
        private async Task KeepSocketOpen(WebSocket socket)
        {
            var buffer = new byte[1024 * 4];
            try
            {
                while (socket.State == WebSocketState.Open)
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.WriteLine($"Received message: {message}");
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Console.WriteLine("Closing WebSocket connection.");
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", CancellationToken.None);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in WebSocket handling: {ex.Message}");
            }
        }

        // Join an existing quiz room (WebSocket connection for participants)
        [Route("join/{roomId}")]
        public async Task JoinRoom(string roomId, [FromQuery] string username)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                Console.WriteLine($"User {username} attempting to join room {roomId}.");

                if (_roomManager.JoinRoom(roomId, username, socket))
                {
                    Console.WriteLine($"User {username} joined room {roomId} successfully.");
                    await Task.Delay(500000);
                }
                else
                {
                    Console.WriteLine($"Failed to join room {roomId}. Room does not exist.");
                    var failureMessage = $"Failed to join room {roomId}. Room may not exist.";
                    var messageBytes = Encoding.UTF8.GetBytes(failureMessage);
                    await socket.SendAsync(messageBytes, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
            else
            {
                HttpContext.Response.StatusCode = 400; // Bad Request
                Console.WriteLine("Failed to join room: Not a WebSocket request.");
            }
        }

        // Get a list of active quiz rooms
        [HttpGet("list")]
        public IActionResult GetQuizRooms()
        {
            var activeRooms = _roomManager.GetActiveRooms();
            if (activeRooms.Count == 0)
            {
                return Ok(new { message = "No available quiz rooms at the moment." });
            }
            return Ok(activeRooms);
        }
    }
}
