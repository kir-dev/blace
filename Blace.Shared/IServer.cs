using Blace.Shared.Models;

namespace Blace.Shared;

public interface IServer
{
    Task<Place> GetPlace();
    Task<uint> GetCooldown();
    Task PlaceTile(int x, int y, byte color);
    Task<List<Tile>?> GetTilesBySamePlayer(int x, int y, byte color);
    Task DeleteTiles(Tile[] tiles);
}