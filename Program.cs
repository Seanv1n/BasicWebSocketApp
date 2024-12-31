using System.Collections.Concurrent;
using System.Net.WebSockets;
using WebSocketApp.Services;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseWebSockets();

var userConnections = new ConcurrentDictionary<string, WebSocket>();

app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var userId = context.Request.Query["user"];
        if (string.IsNullOrEmpty(userId))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("User ID is required");
            return;
        }

        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        Console.WriteLine("WebSocket connection established.");

        userConnections.TryAdd(userId, webSocket);
        var webSocketService = new WebSocketService();
        await webSocketService.HandleWebSocketConnection(userId, webSocket, userConnections);

        userConnections.TryRemove(userId, out _);
        Console.WriteLine($"User {userId} disconnected.");
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.Run();
