using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace WebSocketApp.Commands
{
    public class SpecialCommands : BaseCommand
    {
        // Method to broadcast a message to all users except the sender
        public async Task Broadcast(string message, ConcurrentDictionary<string, WebSocket> userConnections, string userId)
        {
            foreach (var connection in userConnections)
            {
                if (connection.Key != userId)
                {
                    try
                    {
                        var broadcastMessage = $"{userId} says: {message}";
                        await SendMessage(broadcastMessage, connection.Value);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error broadcasting to {connection.Key}: {ex.Message}");
                    }
                }
            }
        }

        // Method to send a whisper message to a target user
        public async Task Whisper(string userId, string targetUserId, string message, ConcurrentDictionary<string, WebSocket> userConnections)
        {
            if (userConnections.TryGetValue(targetUserId, out var targetWebSocket))
            {
                try
                {
                    var whisperMessage = $"{targetUserId} whispers: {message}";
                    await SendMessage(whisperMessage, targetWebSocket);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error whispering to {targetUserId}: {ex.Message}");
                }
            }
            else
            {
                await SendMessage($"User {targetUserId} not found.", userConnections[userId]);
            }
        }

    }
}

