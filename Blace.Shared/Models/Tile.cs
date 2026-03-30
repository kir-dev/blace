namespace Blace.Shared.Models;

public class Tile
{
    public int Id { get; set; }
    
    public int? DeleteId { get; set; }
    public int PlaceId { get; init; }
    public int UserId { get; init; }
    
    public DateTime CreatedTimeUtc { get; init; }
    public ushort X { get; init; }
    public ushort Y { get; init; }
    public byte Color { get; init; }
    public byte PreviousColor { get; init; }
    
    public Delete? Delete { get; set; }
    public Place Place { get; set; } = null!;
    public User User { get; set; } = null!;
}