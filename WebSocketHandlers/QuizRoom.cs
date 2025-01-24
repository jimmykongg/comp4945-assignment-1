using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace QuizApp.WebSocketHandlers;

public class QuizRoom
{
    public string RoomId { get; }
    public int CategoryId { get; }
    public string HostUsername { get; }
    private readonly WebSocket _hostSocket;
    private readonly List<Quiz> _questions = new(); 
    private readonly ConcurrentDictionary<string, Participant> _participants = new();
    private readonly QuizRoomManager _quizManager;
    private int _currentQuestionIndex = 0; 

    public QuizRoom(string roomId, int categoryId, string hostUsername, WebSocket hostSocket, QuizRoomManager quizManager)
    {
        RoomId = roomId;
        CategoryId = categoryId;
        HostUsername = hostUsername;
        _hostSocket = hostSocket;
    }
    
    /**
     * The main logic of Quiz Room
     * THe "Run" method for a thread class in Java
     */
    public async Task StartRoom()
    {
        Console.WriteLine("QuizRoomTask: Started, loading questions...");
        await LoadQuestions();
        Console.WriteLine("QuizRoomTask: Questions loaded.");
        while (_currentQuestionIndex < _questions.Count)
        {
            var currentQuestion = _questions[_currentQuestionIndex];

            // Notify host and participants
            var message = new
            {
                type = "question",
                data = currentQuestion
            };

            Console.WriteLine("QuizRoomTask: Sending Current Question...");
            await SendToHostAsync(message);
            await BroadcastToParticipantsAsync(message);

            Console.WriteLine("QuizRoomTask: Waiting for host messages...");
            await WaitForHostToAdvance();
            Console.WriteLine("QuizRoomTask:Now move on to next question...");
            _currentQuestionIndex++;
        }

        await NotifyEndOfQuiz();
        _quizManager.CloseRoom(RoomId);
    }
    
    public bool AddParticipant(string username, WebSocket webSocket)
    {
        Console.WriteLine($"Adding participant: {username}");
        if (_participants.TryAdd(username, new Participant(username, webSocket)))
        {
            NotifyHostNewParticipant(username);

            if (_currentQuestionIndex < _questions.Count)
            {
                SendQuestionToClient(webSocket, _questions[_currentQuestionIndex]);
                _ = Task.Run(() => ListenToClient(webSocket, username));
            }
            return true;
        }
        return false;
    }

    public async Task ListenToClient(WebSocket webSocket, string username)
    {
        var buffer = new byte[1024 * 4];

        while (webSocket.State == WebSocketState.Open)
        {
            Console.WriteLine("Listening to " + username + "...");
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"Received message from {username}: {message}");

                var clientMessage = JsonSerializer.Deserialize<ClientMessage>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (clientMessage?.SelectedAnswerId != null)
                {
                    var notifyHostMessage = new
                    {
                        type = "answerSelected",
                        data = new {selectedAnswerId = clientMessage.SelectedAnswerId }
                    };

                    Console.WriteLine($"Sending message: {notifyHostMessage} to host");
                    await SendToHostAsync(notifyHostMessage);
                }
            }
            else if (result.MessageType == WebSocketMessageType.Close)
            {
                Console.WriteLine($"Client {username} disconnected.");
                _participants.TryRemove(username, out _);
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
            }
        }
    }
    
    private async Task LoadQuestions()
    {
        try
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"http://localhost:5276/api/quiz/category/{CategoryId}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            var fetchedQuestions = JsonSerializer.Deserialize<List<Quiz>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (fetchedQuestions != null)
            {
                _questions.AddRange(fetchedQuestions);
                Console.WriteLine("QuizRoom: Questions Loaded");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading questions: {ex.Message}");
        }
    }
    
    private async void NotifyHostNewParticipant(string username)
    {
        var message = new
        {
            type = "newParticipant",
            data = new { username }
        };

        await SendToHostAsync(message);
    }

    private async Task SendQuestionToClient(WebSocket webSocket, Quiz quiz)
    {
        var message = new
        {
            type = "question",
            data = quiz
        };

        await SendAsync(webSocket, message);
    }

    private async Task NotifyEndOfQuiz()
    {
        var endMessage = new { type = "endQuiz", data = "Quiz ended. Thank you for participating!" };
        await SendToHostAsync(endMessage);
        await BroadcastToParticipantsAsync(endMessage);
    }

    private async Task WaitForHostToAdvance()
    {
        var buffer = new byte[1024 * 4];
        while (true)
        {
            var result = await _hostSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            Console.WriteLine($"Received message bytes from host.");
            if (result.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine("1");
                try
                {
                    var jsonMessage = JsonSerializer.Deserialize<Dictionary<string, string>>(message);
                    if (jsonMessage != null && jsonMessage.ContainsKey("type") && jsonMessage["type"] == "next")
                    {
                        Console.WriteLine("Moving on to next question");
                        break; // Exit the loop when the "next" message is received
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Failed to parse message: {ex.Message}");
                }
            }
        }
    }

    private async Task BroadcastToParticipantsAsync(object message)
    {
        Console.WriteLine("Broadcasting to participants");
        var messageJson = JsonSerializer.Serialize(message);
        var messageBytes = Encoding.UTF8.GetBytes(messageJson);

        foreach (var participant in _participants.Values)
        {
            if (participant.Socket.State == WebSocketState.Open)
            {
                Console.WriteLine($"!!!Sending message: {messageJson} to {participant.Username} for a new page");
                await participant.Socket.SendAsync(messageBytes, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }

    private async Task SendToHostAsync(object message)
    {
        try
        {
            var messageJson = JsonSerializer.Serialize(message);
            var messageBytes = Encoding.UTF8.GetBytes(messageJson);

            if (_hostSocket.State == WebSocketState.Open)
            {
                await _hostSocket.SendAsync(messageBytes, WebSocketMessageType.Text, true, CancellationToken.None);
                Console.WriteLine("Message sent to host.");
            }
            else
            {
                Console.WriteLine("Host socket is closed or not open.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message to host: {ex.Message}");
        }
    }

    private async Task SendAsync(WebSocket webSocket, object message)
    {
        var messageJson = JsonSerializer.Serialize(message);
        var messageBytes = Encoding.UTF8.GetBytes(messageJson);

        if (webSocket.State == WebSocketState.Open)
        {
            await webSocket.SendAsync(messageBytes, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    public int GetParticipantCount()
    {
        return _participants.Count;
    }
}


public class Quiz
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public Media? Media { get; set; }
    public List<Answer> Answers { get; set; } = new();
}

public class Media
{
    public string MediaType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
}

public class Answer
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool RightAnswer { get; set; }
}

public class Participant
{
    public string Username { get; }
    public WebSocket Socket { get; }

    public Participant(string username, WebSocket socket)
    {
        Username = username;
        Socket = socket;
    }
}

public class ClientMessage
{
    public int? SelectedAnswerId { get; set; }
}
