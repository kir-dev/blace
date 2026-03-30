using Blace.Server.Services;
using Blace.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Blace.Server.Data;

public class Db : DbContext, IPlaceRepository
{
    public List<PlaceInfo> Places { get; }
    public async Task<Place> Get(string placeId)
    {
        throw new NotImplementedException();
    }

    public async Task Save(Place place)
    {
        throw new NotImplementedException();
    }

    public async Task SaveTiles(IEnumerable<Tile> tiles)
    {
        throw new NotImplementedException();
    }

    public async Task Delete(PlaceInfo place)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Tile>> GetTilesBySamePlayer(int x, int y, byte color, string placeId)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteTiles(Tile[] tiles)
    {
        throw new NotImplementedException();
    }
}