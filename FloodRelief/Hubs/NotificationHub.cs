using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FloodRelief.Hubs;

[Authorize(Roles = "User,Staff")]
public sealed class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var principalId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(principalId))
        {
            Context.Abort();
            return;
        }

        string? groupName = null;

        if (Context.User?.IsInRole("Staff") == true)
        {
            groupName = $"staff:{principalId}";
        }
        else if (Context.User?.IsInRole("User") == true)
        {
            groupName = $"user:{principalId}";
        }

        if (string.IsNullOrWhiteSpace(groupName))
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await base.OnConnectedAsync();
    }
}
