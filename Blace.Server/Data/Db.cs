using Blace.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Blace.Server.Data;

public class Db(DbContextOptions<Db> options) : DbContext(options)
{
    public DbSet<Place> Places { get; set; } = null!;
    public DbSet<Tile> Tiles { get; set; } = null!;
    public DbSet<Delete> Deletes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Place>(entity =>
        {
            entity.Property(p => p.Id).HasMaxLength(450);
            entity.Property(p => p.Title).HasMaxLength(200);
            entity.Property(p => p.Canvas).HasColumnType("bytea");
        });

        modelBuilder.Entity<Tile>(entity =>
        {
            entity.Property(t => t.Id).HasMaxLength(450);
            entity.Property(t => t.PlaceId).HasMaxLength(450);
            entity.Property(t => t.DeleteId).HasMaxLength(450);
            
            entity.HasIndex(t => t.PlaceId);
            entity.HasIndex(t => new { t.PlaceId, t.X, t.Y });
            entity.HasIndex(t => new { t.PlaceId, t.UserId });
            entity.HasIndex(t => t.DeleteId);
        });

        modelBuilder.Entity<Delete>(entity =>
        {
            entity.Property(d => d.Id).HasMaxLength(450);
        });

        modelBuilder.Entity<PlaceInfo>(entity =>
        {
            entity.ToTable("places");
            entity.Property(p => p.Id).HasMaxLength(450);
            entity.Property(p => p.Title).HasMaxLength(200);
        });
    }
}