using Blace.Shared.Models;

namespace Blace.Shared;

public interface IClient
{
    Task UpdatePlayers(List<Player> players) => Task.CompletedTask;
    Task UpdatePixels(Pixel[] pixels) => Task.CompletedTask;
    Task UpdatePlace(Place place) => Task.CompletedTask;
    Task UpdateCooldown(uint cooldown) => Task.CompletedTask;
}
