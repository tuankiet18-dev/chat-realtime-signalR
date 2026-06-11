using System.Security.Claims;
using ChatApp.Contracts;
using ChatApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Hubs;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class ChatHub(ChatService chatService, PresenceService presenceService) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var user = new PresenceUser(
            CurrentUser.GetRequiredUserId(Context.User!),
            Context.User!.FindFirstValue(ClaimTypes.Name) ?? "Unknown",
            Context.User!.FindFirstValue(ClaimTypes.Role) ?? "User");
        var users = presenceService.Connect(Context.ConnectionId, user);

        await Clients.Caller.SendAsync("RecentMessagesLoaded",
            await chatService.GetRecentMessagesAsync(user.UserId, CurrentUser.IsAdmin(Context.User!)));
        await Clients.All.SendAsync("PresenceChanged", users);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Clients.All.SendAsync("PresenceChanged", presenceService.Disconnect(Context.ConnectionId));
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(SendMessageRequest request)
    {
        var userId = CurrentUser.GetRequiredUserId(Context.User!);
        var isAdmin = CurrentUser.IsAdmin(Context.User!);
        var message = await chatService.SendMessageAsync(userId, isAdmin, request);
        if (message is not null)
        {
            await Clients.All.SendAsync("MessageReceived", message);
        }
    }

    public async Task RecallMessage(Guid messageId)
    {
        var userId = CurrentUser.GetRequiredUserId(Context.User!);
        var message = await chatService.RecallMessageAsync(messageId, userId, CurrentUser.IsAdmin(Context.User!));
        if (message is not null)
        {
            await Clients.All.SendAsync("MessageRecalled", message);
        }
    }

    [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task AdminDeleteMessage(Guid messageId)
    {
        var userId = CurrentUser.GetRequiredUserId(Context.User!);
        var message = await chatService.AdminDeleteMessageAsync(messageId, userId);
        if (message is not null)
        {
            await Clients.All.SendAsync("MessageDeleted", message);
        }
    }

    public async Task Typing(bool isTyping)
    {
        var state = new TypingState(
            CurrentUser.GetRequiredUserId(Context.User!),
            Context.User!.FindFirstValue(ClaimTypes.Name) ?? "Unknown",
            isTyping);
        await Clients.Others.SendAsync("TypingChanged", state);
    }

    public async Task MarkSeen(Guid messageId)
    {
        var userId = CurrentUser.GetRequiredUserId(Context.User!);
        var message = await chatService.MarkSeenAsync(messageId, userId, CurrentUser.IsAdmin(Context.User!));
        if (message is not null)
        {
            await Clients.All.SendAsync("ReadReceiptsChanged", message);
        }
    }
}
