using Blace.Server.Services;
using Blace.Shared;
using Blace.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Blace.Server;

public class Server(
    PlayerCountService playerCountService,
    PlaceService placeService,
    IAuthorizationService authorizationService
)
    : Hub<IClient>, IServer
{
    public override Task OnConnectedAsync()
    {
        playerCountService.OnConnected(Context);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception != null)
            SentrySdk.CaptureException(exception);
        playerCountService.OnDisconnected(Context);
        return Task.CompletedTask;
    }

    public Task<Place> GetPlace() => Task.FromResult(placeService.Place);
    public Task<uint> GetCooldown() => Task.FromResult(placeService.Cooldown);

    public Task PlaceTile(int x, int y, byte color)
    {
        if (Context.User is not { Identity.IsAuthenticated: true } user)
            return Task.CompletedTask;
        placeService.SetPixel(x, y, color, user.GetUserId());
        return Task.CompletedTask;
    }

    public async Task<List<Tile>?> GetTilesBySamePlayer(int x, int y, byte color)
    {
        try
        {
            return await placeService.GetTilesBySamePlayer(x, y, color);
        }
        catch (TileNotFoundException)
        {
            return null;
        }
    }

    public async Task DeleteTiles(Tile[] tiles)
    {
        var authorizationResult = await authorizationService.AuthorizeAsync(Context.User!, Constants.AdminPolicy);
        if (!authorizationResult.Succeeded)
            return;
        await placeService.DeleteTiles(tiles);
    }
}