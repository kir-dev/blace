using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Blace.Server;

public static class Extensions
{
    public static int GetId(this HubCallerContext context)
        => int.Parse(context.UserIdentifier ?? throw new("UserId is null"));
    
    public static Guid? GetAuthSchId(this ClaimsPrincipal claimsPrincipal)
    {
        string? value = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        if (value != null)
            return Guid.Parse(value);
        return null;
    }
}