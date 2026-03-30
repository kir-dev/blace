namespace Blace.Shared.Models;

public class User
{
    public int Id { get; set; }
    
    public bool IsBanned { get; set; }

    public List<Tile> Tiles { get; } = [];
    public List<Delete> Deletes { get; } = [];
    public Guid AuthSchId { get; set; }
}