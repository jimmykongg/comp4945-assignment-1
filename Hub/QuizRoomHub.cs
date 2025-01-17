using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuizApp.Hubs
{
    public class QuizRoomHub : Hub
    {
        // A dictionary to track users by their connection ID in each quiz room
        private static Dictionary<string, List<string>> _quizRooms = new Dictionary<string, List<string>>();

        // When a user connects, they will join a room
        public async Task JoinRoom(string roomName, string username)
        {
            if (!_quizRooms.ContainsKey(roomName))
            {
                _quizRooms[roomName] = new List<string>();
            }

            // Add the user's connection ID to the room's participant list (not username)
            _quizRooms[roomName].Add(Context.ConnectionId);

            // Notify all clients in the room about the new user
            await Clients.Group(roomName).SendAsync("UserJoined", username);

            // Add the user to the SignalR room group (this is the SignalR "room")
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        }

        // When a user sends a message to the room
        public async Task SendMessage(string roomName, string message)
        {
            await Clients.Group(roomName).SendAsync("ReceiveMessage", message);
        }

        // When a user leaves the room
        public async Task LeaveRoom(string roomName, string username)
        {
            if (_quizRooms.ContainsKey(roomName))
            {
                _quizRooms[roomName].Remove(Context.ConnectionId);
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);

            // Notify others that the user left
            await Clients.Group(roomName).SendAsync("UserLeft", username);
        }

        // Optional: Handle connection disconnected event
        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            // Logic to remove the user from the room when they disconnect
            foreach (var room in _quizRooms)
            {
                if (room.Value.Contains(Context.ConnectionId))
                {
                    // You can remove the user from the room based on their connection ID
                    await LeaveRoom(room.Key, "Unknown"); // Placeholder for username
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
