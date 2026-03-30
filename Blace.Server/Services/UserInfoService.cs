using System.Data;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using Blace.Server.Data;
using Blace.Shared.Models;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;

namespace Blace.Server.Services;

public class UserInfoService(Db db)
{
    public async Task OnUserInformationReceived(UserInformationReceivedContext context)
    {
        Guid authSchId = context.Principal!.GetAuthSchId()!.Value;

        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        
        User user = await db.Users
                        .FirstOrDefaultAsync(u => u.AuthSchId == authSchId)
                    ?? db.Users.Add(new() { AuthSchId = authSchId }).Entity;

        AuthSchUserInfo userInfo = context.User.Deserialize<AuthSchUserInfo>(JsonSerializerOptions.Web)!;

        // add claims to the user's cookie
        ClaimsIdentity identity = (ClaimsIdentity)context.Principal!.Identity!;

        await db.SaveChangesAsync();
        await tx.CommitAsync();

        identity.AddClaim(new(
            Constants.UserIdClaim,
            user.Id.ToString(CultureInfo.InvariantCulture)
        ));
        // TODO: Add admin role
    }
}
