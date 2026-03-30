namespace Blace.Shared.Models;

public class Delete
{
    public int Id { get; set; }
    public DateTime DateTimeUtc { get; init; }
    public int UserId { get; init; }

    public User User { get; set; } = null!;
}