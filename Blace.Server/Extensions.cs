using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Blace.Server;

public static class Extensions
{
    extension(ClaimsPrincipal claimsPrincipal)
    {
        public int GetUserId()
        {
            string? value = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == Constants.UserIdClaim)?.Value;
            return value != null
                ? int.Parse(value)
                : throw new InvalidOperationException("User ID not found in claims");
        }

        public Guid? GetAuthSchId()
        {
            string? value = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (value != null)
                return Guid.Parse(value);
            return null;
        }
    }
}