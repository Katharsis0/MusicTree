using System.ComponentModel.DataAnnotations;

namespace MusicTree.Models.Entities;


//Representación  del objeto en la DB
public class Cluster
{
    [Key]
    public string Id { get; set; } = $"C-{GenerateRandomId()}";
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    public List<Genre> Genres { get; set; } = new List<Genre>();
    
    
    private static string GenerateRandomId()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 12)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}