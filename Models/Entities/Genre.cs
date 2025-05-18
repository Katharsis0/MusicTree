namespace MusicTree.Models.Entities;

public class Genre
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool IsSubgenre { get; set; }
    
    // Navigation properties
    public string? ParentGenreId { get; set; }
    public Genre? ParentGenre { get; set; }
    
    public string? ClusterId { get; set; }
    public Cluster? Cluster { get; set; }
    
    public List<Genre> RelatedGenres { get; set; } = new List<Genre>();
    
    //Musical attributes
    public float GenreTipicalMode { get; set; }
    public int Bpm { get; set; }
    public int Volume { get; set; }
    public int CompasMetric { get; set; }
    public int AvrgDuration { get; set; }
    
    //Optional
    public string? Color { get; set; }
    public string? GenreCreationYear { get; set; }
    public string? GenreOriginCountry { get; set; }
    
    //Metadata
    public bool IsActive { get; set; } = true;
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
}