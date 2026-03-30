namespace Blace.Shared.Models;

public class Delete
{
    public int Id { get; set; }
    public int UserId { get; init; }
    public DateTime DateTimeUtc { get; init; }

    public User User { get; set; } = null!;
    public List<Tile> Tiles { get; } = [];
}