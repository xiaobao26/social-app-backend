using Microsoft.AspNetCore.SignalR;
using social_app_backend.Models;

namespace social_app_backend.Hubs;

public class ChatHub: Hub
{
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(ILogger<ChatHub> logger)
    {
        _logger = logger;
    }
    /// <summary>
    /// user join the group
    /// broadcast message for all joined people in this room
    /// </summary>
    /// <param name="userConnection"></param>
    public async Task JoinChatroom(UserConnection userConnection)
    {
        _logger.LogInformation($"{userConnection.Username} {userConnection.RoomNumber}");
        await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.RoomNumber);
        await Clients.Group(userConnection.RoomNumber).SendAsync("ReceiveMessage", "Admin", $"👤 {userConnection.Username} has joined this chatroom.");
    }


    public async Task SendChatMessage(UserConnection userConnection, string message)
    {
        _logger.LogInformation($"{message}");
        await Clients.Group(userConnection.RoomNumber).SendAsync("ReceiveChatMessage", userConnection.Username, message);
    }
    
    
    /// <summary>
    /// user leave the group
    /// broadcast message
    /// </summary>
    /// <param name="userConnection"></param>
    public async Task OnDisconnectedAsync(UserConnection userConnection)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, userConnection.RoomNumber);
        await Clients.Group(userConnection.RoomNumber).SendAsync("ReceiveMessage", "Admin", $"{userConnection.Username} leaving this chatroom.");
    }
}