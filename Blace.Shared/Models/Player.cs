namespace Blace.Shared.Models;

public class Player
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public DateTimeOffset JoinTime { get; } = DateTimeOffset.UtcNow;
    public int Score { get; set; }
    public bool IsHidden { get; set; }
    public bool IsConnected { get; set; } = true;
}
