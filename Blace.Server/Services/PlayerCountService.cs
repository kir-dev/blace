using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Blace.Shared;
using Blace.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace Blace.Server.Services;

public class PlayerCountService
{
    private readonly Throttler _throttler;
    private readonly ConcurrentDictionary<int, int> _players = new();
    private int _playerCount;

    public PlayerCountService(IHubContext<Server, IClient> hub)
    {
        _throttler = new(
            1000, 
            () => hub.Clients.All.UpdatePlayerCount(_playerCount)
        );
    }


    public void Update() => _throttler.Update();

    public void OnConnected(HubCallerContext context)
    {
        if (context.User is not { Identity.IsAuthenticated: true } user)
            return;
        int userId = user.GetUserId();
        int newConnectionCount = _players.AddOrUpdate<object>(
            userId,
            static (_, _) => 1,
            static (_, count, _) => count + 1,
            null!
        );
        if (newConnectionCount == 1)
            Interlocked.Increment(ref _playerCount);
    }
    
    public void OnDisconnected(HubCallerContext context)
    {
        if (context.User is not { Identity.IsAuthenticated: true } user)
            return;
        int userId = user.GetUserId();
        int newConnectionCount = _players.AddOrUpdate<object>(
            userId,
            static (_, _) => throw new InvalidOperationException("Disconnected without connecting first"),
            static (_, count, _) => count - 1,
            null!
        );
        if (newConnectionCount == 0)
            Interlocked.Decrement(ref _playerCount);
    }
}