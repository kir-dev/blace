namespace Blace.Shared.Models;

public class PlaceInfo
{
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTime LastChangeTimeUtc { get; set; }
    public DateTime CreatedTimeUtc { get; init; }
}