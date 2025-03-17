using Azure;
using Azure.AI.Translation.Text;
using Microsoft.AspNetCore.SignalR;
using social_app_backend.Models;

namespace social_app_backend.Hubs;

public class ChatHub: Hub
{
    private readonly ILogger<ChatHub> _logger;
    private readonly TextTranslationClient _textTranslationClient;

    public ChatHub(ILogger<ChatHub> logger)
    {
        _logger = logger;

        string apiKey = "9WuQ4X5EMXqYWlUpbVqkDRchVLBnthwfhZnVLStwpsRMv7ygjLkJJQQJ99BCACL93NaXJ3w3AAAbACOGqRYu";
        string region = "australiaeast";
        _textTranslationClient = new TextTranslationClient(new AzureKeyCredential(apiKey), region);
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

    public async Task TranslateMessage(string message, string targetLanguage)
    {
        try
        {
            var response = await _textTranslationClient.TranslateAsync(targetLanguage, message);
            _logger.LogInformation($"{response.Value}");
            var translatedMessage = response.Value.First().Translations.First().Text;
            await Clients.Client(Context.ConnectionId)
                .SendAsync("ReceiveTranslatedMessage", message, translatedMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError($"{ex.Message} {ex.StackTrace}");
        }
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