namespace MusicTree.Models.Entities;

public class Cluster
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    public List<Genre> Genres { get; set; } = new List<Genre>();
}