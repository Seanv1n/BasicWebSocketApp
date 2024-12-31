using System.Net.WebSockets;
using System.Text;

namespace WebSocketApp.Commands
{
    public class BaseCommand
    {
        // Method to send a message to a specific WebSocket connection
        public async Task SendMessage(string message, WebSocket connection)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            try
            {
                await connection.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }
    }

}
