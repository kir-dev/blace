using Blace.Server.Services;
using Blace.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Blace.Server.Data;

public class Db : DbContext, IPlaceRepository
{
    public Db(DbContextOptions<Db> options) : base(options)
    {
    }

    public DbSet<Place> PlacesDb { get; set; } = null!;
    public DbSet<Tile> Tiles { get; set; } = null!;
    public DbSet<Delete> Deletes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Place entity
        modelBuilder.Entity<Place>(entity =>
        {
            entity.ToTable("places");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).HasMaxLength(450);
            entity.Property(p => p.Title).HasMaxLength(200);
            entity.Property(p => p.CreatedTimeUtc).IsRequired();
            entity.Property(p => p.LastChangeTimeUtc).IsRequired();
            entity.Property(p => p.Canvas).HasColumnType("bytea");
            entity.Property(p => p.Height).IsRequired();
            entity.Property(p => p.Width).IsRequired();
        });

        // Configure Tile entity
        modelBuilder.Entity<Tile>(entity =>
        {
            entity.ToTable("tiles");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).HasMaxLength(450);
            entity.Property(t => t.PlaceId).HasMaxLength(450).IsRequired();
            entity.Property(t => t.UserId).IsRequired();
            entity.Property(t => t.CreatedTimeUtc).IsRequired();
            entity.Property(t => t.X).IsRequired();
            entity.Property(t => t.Y).IsRequired();
            entity.Property(t => t.Color).IsRequired();
            entity.Property(t => t.PreviousColor).IsRequired();
            entity.Property(t => t.DeleteId).HasMaxLength(450);
            
            entity.HasIndex(t => t.PlaceId);
            entity.HasIndex(t => new { t.PlaceId, t.X, t.Y });
            entity.HasIndex(t => new { t.PlaceId, t.UserId });
            entity.HasIndex(t => t.DeleteId);
        });

        // Configure Delete entity
        modelBuilder.Entity<Delete>(entity =>
        {
            entity.ToTable("deletes");
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Id).HasMaxLength(450);
            entity.Property(d => d.UserId).IsRequired();
            entity.Property(d => d.DateTimeUtc).IsRequired();
        });

        // Configure PlaceInfo as part of Place hierarchy
        modelBuilder.Entity<PlaceInfo>(entity =>
        {
            entity.ToTable("places");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).HasMaxLength(450);
            entity.Property(p => p.Title).HasMaxLength(200);
            entity.Property(p => p.CreatedTimeUtc).IsRequired();
            entity.Property(p => p.LastChangeTimeUtc).IsRequired();
        });
    }

    public List<PlaceInfo> Places => PlacesDb.OrderByDescending(p => p.CreatedTimeUtc).Cast<PlaceInfo>().ToList();

    public async Task<Place> Get(string placeId)
    {
        Place? place = await PlacesDb.FindAsync(placeId);
        if (place == null)
        {
            throw new InvalidOperationException($"Place with ID '{placeId}' not found.");
        }

        // Handle legacy data where Width might be 0
        if (place.Width == 0)
        {
            place.Height = place.Width = 128;
        }

        return place;
    }

    public async Task Save(Place place)
    {
        // Get existing title if it exists
        Place? existing = await PlacesDb.FindAsync(place.Id);
        if (existing != null)
        {
            place.Title = existing.Title;
        }

        place.LastChangeTimeUtc = DateTime.UtcNow;

        if (existing != null)
        {
            Entry(existing).CurrentValues.SetValues(place);
        }
        else
        {
            PlacesDb.Add(place);
        }

        await SaveChangesAsync();
    }

    public async Task SaveTiles(IEnumerable<Tile> tiles)
    {
        await Tiles.AddRangeAsync(tiles);
        await SaveChangesAsync();
    }

    public async Task Delete(PlaceInfo place)
    {
        PlaceInfo? existing = await PlacesDb.FindAsync(place.Id);
        if (existing != null)
        {
            PlacesDb.Remove((Place)existing);
            await SaveChangesAsync();
        }
    }

    public async Task<List<Tile>> GetTilesBySamePlayer(int x, int y, byte color, string placeId)
    {
        // Find the last tile at the specified position with the specified color
        Tile? lastTile = await Tiles
            .Where(t => t.PlaceId == placeId && t.Color == color && t.X == x && t.Y == y && t.DeleteId == null)
            .OrderByDescending(t => t.CreatedTimeUtc)
            .FirstOrDefaultAsync();

        if (lastTile == null)
        {
            throw new TileNotFoundException();
        }

        Guid userId = lastTile.UserId;

        // Get all tiles by the same user in the same place that are not deleted
        List<Tile> tiles = await Tiles
            .Where(t => t.PlaceId == placeId && t.UserId == userId && t.DeleteId == null)
            .OrderByDescending(t => t.CreatedTimeUtc)
            .ToListAsync();

        return tiles;
    }

    public async Task DeleteTiles(Tile[] tiles)
    {
        if (tiles.Length == 0) return;

        Guid userId = tiles[0].UserId;
        string placeId = tiles[0].PlaceId;

        if (tiles.Any(t => t.UserId != userId || t.PlaceId != placeId))
        {
            throw new InvalidOperationException("All of the tiles must have the same UserId and PlaceId.");
        }

        // Create a delete record
        Delete delete = new(Guid.NewGuid().ToString(), DateTime.UtcNow, userId);
        await Deletes.AddAsync(delete);

        // Update all tiles with the delete ID (soft delete)
        foreach (Tile tile in tiles)
        {
            Tile? existingTile = await Tiles.FindAsync(tile.Id);
            if (existingTile != null)
            {
                existingTile.DeleteId = delete.Id;
            }
        }

        await SaveChangesAsync();

        // Update the passed-in tiles to reflect the change
        Array.ForEach(tiles, t => t.DeleteId = delete.Id);
    }
}