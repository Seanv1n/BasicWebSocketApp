using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using WebSocketApp.Commands;

namespace WebSocketApp.Services;

public class WebSocketService
{
    private readonly SpecialCommands _specialCommands = new SpecialCommands();
    // Main method to handle WebSocket connections and messages
    public async Task HandleWebSocketConnection(string userId, WebSocket webSocket, ConcurrentDictionary<string, WebSocket> userConnections)
    {
        var buffer = new byte[1024 * 4];

        try
        {
            // Broadcast user login
            await _specialCommands.Broadcast($"{userId} has logged in", userConnections, userId);

            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine($"WebSocket connection closed by {userId}.");
                    await _specialCommands.Broadcast($"{userId} has logged off", userConnections, userId);
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    break;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"Received from {userId}: {message}");

                // Handle special commands here
                if (message.StartsWith("%w "))
                {
                    var parts = message.Substring(3).Split(' ', 2);
                    if (parts.Length == 2)
                    {
                        var targetUserId = parts[0];
                        var whisperMessage = parts[1];
                        await _specialCommands.Whisper(userId, targetUserId, whisperMessage, userConnections);
                    }
                    else
                    {
                        await _specialCommands.SendMessage("Usage: %w {userId} {message}", webSocket);
                    }
                }
                else if (message == "%q")
                {
                    Console.WriteLine($"{userId} initiated connection close.");
                    await _specialCommands.Broadcast($"{userId} has logged off", userConnections, userId);
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    break;
                }
                else
                {
                    // Default: Broadcast the message
                    await _specialCommands.Broadcast(message, userConnections, userId);
                }
            }
        }
        finally
        {
            // Clean up: remove the user connection when the loop ends
            if (webSocket.State != WebSocketState.Closed)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }
            userConnections.TryRemove(userId, out _);
            Console.WriteLine($"User {userId} disconnected.");
        }
    }

}
