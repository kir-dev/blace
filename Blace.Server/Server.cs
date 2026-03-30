using Blace.Server.Services;
using Blace.Shared;
using Blace.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace Blace.Server;

public class Server(
    PlayerService playerService,
    PlaceService placeService
)
    : Hub<IClient>, IServer
{
    public override Task OnConnectedAsync()
    {
        playerService[Context].IsConnected = true;
        playerService.Update();
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception != null)
            SentrySdk.CaptureException(exception);
        playerService[Context].IsConnected = false;
        playerService.Update();
        return Task.CompletedTask;
    }

    public Task<Player> GetMe() => Task.FromResult(playerService[Context]);
    public Task<Place> GetPlace() => Task.FromResult(placeService.Place);
    public Task<uint> GetCooldown() => Task.FromResult(placeService.Cooldown);

    public Task PlaceTile(int x, int y, byte color)
    {
        placeService.SetPixel(x, y, color, Context.GetId());
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
        if (playerService[Context].Id != PlaceService.AdminUserId)
            return;
        await placeService.DeleteTiles(tiles);
    }

    public Task SetName(string name)
    {
        playerService[Context].Name = name;
        playerService.Update();
        return Task.CompletedTask;
    }
}