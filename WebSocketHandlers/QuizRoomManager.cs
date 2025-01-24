using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace QuizApp.WebSocketHandlers;

public class QuizRoomManager
{
    private readonly ConcurrentDictionary<string, QuizRoom> _rooms = new();

    // Create a new quiz room
    public string CreateRoom(int categoryId, string hostUsername, WebSocket hostSocket)
    {
        Console.WriteLine("QuizRoomManager is creating a new Room");
        var roomId = Guid.NewGuid().ToString(); // Unique room ID
        var room = new QuizRoom(roomId, categoryId, hostUsername, hostSocket, this);
        Console.WriteLine($"QuizRoomManager: Room Has Been Created: {roomId}");
        _rooms.TryAdd(roomId, room);
        Console.WriteLine($"QuizRoomManager: Starting QuizRoom Logic");
        _ = Task.Run(() => room.StartRoom()); // add this line. start the logic after the room is created.
        return roomId;
    }

    // Join an existing room
    public bool JoinRoom(string roomId, string username, WebSocket participantSocket)
    {
        if (_rooms.TryGetValue(roomId, out var room))
        {
            return room.AddParticipant(username, participantSocket);
        }

        return false;
    }

    // Get all active rooms
    public List<object> GetActiveRooms()
    {
        return _rooms.Values.Select(room => new
        {
            RoomId = room.RoomId,
            CategoryId = room.CategoryId,
            Host = room.HostUsername,
            Participants = room.GetParticipantCount()
        }).ToList<object>();
    }

    // Close a room
    public void CloseRoom(string roomId)
    {
        if (_rooms.TryRemove(roomId, out var removedRoom))
        {
            Console.WriteLine($"Room {roomId} removed successfully.");
        }
        else
        {
            Console.WriteLine($"Failed to remove room {roomId}. Room may not exist.");
        }
    }
}