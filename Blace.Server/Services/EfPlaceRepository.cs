using Blace.Server.Data;
using Blace.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Blace.Server.Services;

public class EfPlaceRepository(IDbContextFactory<Db> dbContextFactory) : IPlaceRepository
{
    public List<PlaceInfo> Places { get; private set; } = [];

    public async Task Initialize()
    {
        await using Db db = await dbContextFactory.CreateDbContextAsync();
        
        await db.Database.MigrateAsync();
        
        Places = await db.Places
            .OrderByDescending(p => p.CreatedTimeUtc)
            .Cast<PlaceInfo>()
            .ToListAsync();
    }

    public async Task<Place> Get(int placeId)
    {
        await using Db db = await dbContextFactory.CreateDbContextAsync();
        
        Place? place = await db.Places.FindAsync(placeId);
        return place ?? throw new InvalidOperationException($"Place with ID '{placeId}' not found.");
    }

    public async Task Save(Place place)
    {
        await using Db db = await dbContextFactory.CreateDbContextAsync();
        
        // Get existing title if it exists
        Place? existing = await db.Places.FindAsync(place.Id);
        if (existing != null)
        {
            place.Title = existing.Title;
        }

        place.LastChangeTimeUtc = DateTime.UtcNow;

        if (existing != null)
        {
            db.Entry(existing).CurrentValues.SetValues(place);
        }
        else
        {
            db.Places.Add(place);
            Places.Add(place);
        }

        await db.SaveChangesAsync();
        
        // Update in-memory list
        PlaceInfo? existingInfo = Places.FirstOrDefault(p => p.Id == place.Id);
        existingInfo?.LastChangeTimeUtc = place.LastChangeTimeUtc;
    }

    public async Task SaveTiles(IEnumerable<Tile> tiles)
    {
        await using Db db = await dbContextFactory.CreateDbContextAsync();
        
        await db.Tiles.AddRangeAsync(tiles);
        await db.SaveChangesAsync();
    }

    public async Task Delete(PlaceInfo place)
    {
        await using Db db = await dbContextFactory.CreateDbContextAsync();
        
        PlaceInfo? existing = await db.Places.FindAsync(place.Id);
        if (existing != null)
        {
            db.Places.Remove((Place)existing);
            await db.SaveChangesAsync();
            Places.Remove(place);
        }
    }

    public async Task<List<Tile>> GetTilesBySamePlayer(int x, int y, byte color, int placeId)
    {
        await using Db db = await dbContextFactory.CreateDbContextAsync();
        
        // Find the last tile at the specified position with the specified color
        Tile? lastTile = await db.Tiles
            .Where(t => t.PlaceId == placeId && t.Color == color && t.X == x && t.Y == y && t.DeleteId == null)
            .OrderByDescending(t => t.CreatedTimeUtc)
            .FirstOrDefaultAsync();

        if (lastTile == null)
        {
            throw new TileNotFoundException();
        }

        int userId = lastTile.UserId;

        // Get all tiles by the same user in the same place that are not deleted
        List<Tile> tiles = await db.Tiles
            .Where(t => t.PlaceId == placeId && t.UserId == userId && t.DeleteId == null)
            .OrderByDescending(t => t.CreatedTimeUtc)
            .ToListAsync();

        return tiles;
    }

    public async Task DeleteTiles(Tile[] tiles)
    {
        if (tiles.Length == 0) return;

        await using Db db = await dbContextFactory.CreateDbContextAsync();

        int userId = tiles[0].UserId;
        int placeId = tiles[0].PlaceId;

        if (tiles.Any(t => t.UserId != userId || t.PlaceId != placeId))
        {
            throw new InvalidOperationException("All of the tiles must have the same UserId and PlaceId.");
        }

        // Create a delete record
        Delete delete = new()
        {
            DateTimeUtc = DateTime.UtcNow,
            UserId = userId,
        };
        db.Deletes.Add(delete);

        await db.SaveChangesAsync();

        await db.Tiles
            .Where(t => tiles.Any(t2 => t2 == t))
            .ExecuteUpdateAsync(builder => builder.SetProperty(t => t.DeleteId, delete.Id));
    }
}
