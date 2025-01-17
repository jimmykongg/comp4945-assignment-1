using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuizApp.Hubs
{
    public class QuizRoomHub : Hub
    {
        // A dictionary to track users in each quiz room (this can be replaced with a DB or a more persistent store)
        private static Dictionary<string, List<string>> _quizRooms = new Dictionary<string, List<string>>();

        // When a user connects, they will join a room
        public async Task JoinRoom(string roomName, string username)
        {
            if (!_quizRooms.ContainsKey(roomName))
            {
                _quizRooms[roomName] = new List<string>();
            }

            // Add the user to the room's participant list
            _quizRooms[roomName].Add(username);

            // Notify all clients in the room about the new user
            await Clients.Group(roomName).SendAsync("UserJoined", username);

            // Add the user to the SignalR room group
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
                _quizRooms[roomName].Remove(username);
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
                var user = room.Value.Find(u => u == Context.ConnectionId);
                if (user != null)
                {
                    await LeaveRoom(room.Key, user);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
