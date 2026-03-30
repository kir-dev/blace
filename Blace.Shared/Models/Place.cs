namespace Blace.Shared.Models;

public class Place : PlaceInfo
{
    public int Height { get; set; }
    public int Width { get; set; }
    public byte[]? Canvas { get; init; }
}