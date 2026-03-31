using Blace.Client.Services;
using Blace.Shared;
using Blace.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace Blace.Client;

public class HubClientComponent : ComponentBase, IClient, IDisposable
{
    [Inject] private HubService HubService { get; set; } = null!;
    private IDisposable? _hubRegistration;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _hubRegistration = HubService.RegisterClient(this);
    }
    
    public virtual Task UpdatePixels(Pixel[] pixels) => Task.CompletedTask;
    public virtual Task UpdatePlace(Place place) => Task.CompletedTask;
    public virtual Task UpdateCooldown(uint cooldown) => Task.CompletedTask;
    public virtual Task UpdatePlayerCount(int playerCount) => Task.CompletedTask;

    protected Task StateHasChangedTask()
    {
        StateHasChanged();
        return Task.CompletedTask;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _hubRegistration?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}