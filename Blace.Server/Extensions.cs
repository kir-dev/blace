using Microsoft.AspNetCore.SignalR;

namespace Blace.Server;

public static class Extensions
{
    public static int GetId(this HubCallerContext context)
        => int.Parse(context.UserIdentifier ?? throw new("UserId is null"));
}