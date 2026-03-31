using System.Timers;
using Blazored.LocalStorage;
using Blace.Shared;
using Timer = System.Timers.Timer;

namespace Blace.Client.Services;

public class CooldownService(
    ILocalStorageService localStorageService,
    HubService hubService
)
    : IClient
{
    private readonly IServer _server = hubService.Server;
    private readonly HubService _hubService = hubService;
    private readonly Timer _timer = new(1000);

    private DateTime _lastTilePlacedTime = DateTime.UnixEpoch;
    private uint _cooldown;

    public event Action? RemainingTimeChanged;
    public TimeSpan RemainingTime => _lastTilePlacedTime.AddSeconds(_cooldown).AddMilliseconds(-50) - DateTime.UtcNow;

    public async void OnTilePlaced()
    {
        _lastTilePlacedTime = DateTime.UtcNow;
        OnTimerElapsed(null!, null!);
        _timer.Start();
        await localStorageService.SetItemAsync(nameof(_lastTilePlacedTime), _lastTilePlacedTime);
    }

    public async Task Initialize()
    {
        if (await localStorageService.ContainKeyAsync(nameof(_lastTilePlacedTime)))
            _lastTilePlacedTime = await localStorageService.GetItemAsync<DateTime>(nameof(_lastTilePlacedTime));

        _cooldown = await _server.GetCooldown();

        _timer.Elapsed += OnTimerElapsed;
        _timer.Start();
    }

    public Task UpdateCooldown(uint cooldown)
    {
        _cooldown = cooldown;
        RemainingTimeChanged?.Invoke();
        return Task.CompletedTask;
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (RemainingTime <= TimeSpan.Zero) _timer.Stop();
        RemainingTimeChanged?.Invoke();
    }
}