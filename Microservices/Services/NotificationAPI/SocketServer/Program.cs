using Entities.Concrete;
using RestSharp;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:7002");

var app = builder.Build();
app.UseWebSockets();

var connections = new List<WebSocket>();
var onlineUsers = new Dictionary<string, string>();

app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var curName = context.Request.Query["name"];
        var userId = context.Request.Query["userId"];

        using var ws = await context.WebSockets.AcceptWebSocketAsync();

        onlineUsers[userId.ToString()] = ws.GetHashCode().ToString();

        connections.Add(ws);

        await Broadcast($"{curName} joined the room");
        await Broadcast($"{connections.Count} users connected");

        await ReceiveMessage(ws, async (result, buffer) =>
        {
            if (result.MessageType == WebSocketMessageType.Text)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                if (IsJson(message))
                {
                    var notification = JsonSerializer.Deserialize<Notification>(message);

                    await HandleNotification(notification, ws);
                }
                else
                {
                    await Broadcast(curName + ": " + message);
                }
            }
            else if (result.MessageType == WebSocketMessageType.Close || ws.State == WebSocketState.Aborted)
            {
                connections.Remove(ws);

                await Broadcast($"{curName} left the room");
                await Broadcast($"{connections.Count} users connected");

                await ws.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
        });
    }
    else
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
    }
});

app.Map("/ws/sendNotification", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var curName = context.Request.Query["name"];

        using var ws = await context.WebSockets.AcceptWebSocketAsync();

        await ReceiveMessage(ws, async (result, buffer) =>
        {
            if (result.MessageType == WebSocketMessageType.Text)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                await SendNotification($"Notification from {curName}: {message}");
            }
        });
    }
    else
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
    }
});

async Task SendNotification(string message)
{
    var bytes = Encoding.UTF8.GetBytes(message);

    foreach (var socket in connections)
    {
        if (socket.State == WebSocketState.Open)
        {
            var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
            await socket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}

async Task HandleNotification(Notification notification, WebSocket senderSocket)
{
    string senderSocketId = senderSocket.GetHashCode().ToString();
    var senderDatabaseId = onlineUsers.FirstOrDefault(kv => kv.Value == senderSocketId).Key;

    if (senderDatabaseId != null)
    {
        string receiverKey = notification.Receiver;
        if (onlineUsers.TryGetValue(receiverKey, out var receiverSocketId))
        {
            var receiverSocket = connections.FirstOrDefault(socket =>
                socket.State == WebSocketState.Open &&
                socket.GetHashCode().ToString() == receiverSocketId &&
                socket != senderSocket);

            if (receiverSocket != null)
            {
                // send real-time notification

                var receiverBytes = Encoding.UTF8.GetBytes($"Notification from {notification.Sender}: {notification.Content.Text.Message}");
                var arraySegment = new ArraySegment<byte>(receiverBytes, 0, receiverBytes.Length);
                await receiverSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            else
            {
                await senderSocket.SendAsync(Encoding.UTF8.GetBytes($"Receiver {notification.Receiver} is not connected."), WebSocketMessageType.Text, true, CancellationToken.None);

                // send push notification

                var options = new RestClientOptions("https://onesignal.com/api/v1/notifications");
                var client = new RestClient(options);
                var request = new RestRequest("");

                var appId = Environment.GetEnvironmentVariable("appId");
                var apiKey = Environment.GetEnvironmentVariable("apiKey");

                request.AddHeader("accept", "application/json");
                request.AddHeader("Authorization", $"Basic {apiKey}");
                request.AddJsonBody($"{{\"contents\":{{\"en\":\"{notification.Content.Text.Message}\"}},\"app_id\":\"{appId}\",\"external_id\":\"{receiverKey}\"}}", false);
                var response = await client.PostAsync(request);

                Console.WriteLine("{0}", response.Content);
            }
        }
        else
        {
            await senderSocket.SendAsync(Encoding.UTF8.GetBytes($"Receiver {notification.Receiver} is not valid."), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
    else
    {
        await senderSocket.SendAsync(Encoding.UTF8.GetBytes("Sender information not found."), WebSocketMessageType.Text, true, CancellationToken.None);
    }
}

async Task Broadcast(string message)
{
    var bytes = Encoding.UTF8.GetBytes(message);

    foreach (var socket in connections)
    {
        if (socket.State == WebSocketState.Open)
        {
            var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
            await socket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}

async Task ReceiveMessage(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
{
    var buffer = new byte[1024 * 4];
    while (socket.State == WebSocketState.Open)
    {
        var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        handleMessage(result, buffer);
    }
}

bool IsJson(string input)
{
    return input.TrimStart().StartsWith("{") && input.TrimEnd().EndsWith("}");
}

await app.RunAsync();
