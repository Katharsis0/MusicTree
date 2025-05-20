using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicTree.Models.Entities;

//Representación del objeto en la DB 
public class Genre
{
    [Key] 
    public string Id { get; set; } = string.Empty; //set in constructor

    [Required]
    [StringLength(30, MinimumLength = 3)]
    public string Name { get; set; }

    [StringLength(1000)] 
    public string? Description { get; set; }
    
    public bool IsSubgenre { get; set; }

    public string? ParentGenreId { get; set; }
    public Genre? ParentGenre { get; set; }

    public string? ClusterId { get; set; }
    public Cluster? Cluster { get; set; }

    //Musical attributes
    [Range(-1, 11)] 
    public int Key { get; set; } = -1; // Nota dominante

    // Rango de Beats per minute 
    [Required]
    [Range(0, 250)]
    public int BpmLower { get; set; }
    
    [Required]
    [Range(0, 250)]
    public int BpmUpper { get; set; }

    //Calculated from BpmLower and BpmUpper
    public int Bpm { get; set; }

    // Required 
    [Required]
    [Range(0, 1)]
    public float GenreTipicalMode { get; set; }

    [Required]
    [Range(-60, 0)]
    public int Volume { get; set; }

    [Required]
    [Range(0, 8)]
    public int CompasMetric { get; set; }

    [Required]
    [Range(0, 3600)]
    public int AvrgDuration { get; set; }

    // Optional
    public string? Color { get; set; } // Not for subgenres
    public int? GenreCreationYear { get; set; }
    public string? GenreOriginCountry { get; set; }

    // Related genres
    public ICollection<GenreRelation> RelatedGenresAsSource { get; set; } = new List<GenreRelation>();
    public ICollection<GenreRelation> RelatedGenresAsTarget { get; set; } = new List<GenreRelation>();

    // Get all related genres (not mapped to DB)
    [NotMapped]
    public IEnumerable<Genre> RelatedGenres =>
        RelatedGenresAsSource.Select(r => r.RelatedGenre)
            .Concat(RelatedGenresAsTarget.Select(r => r.Genre));

    //Metadata
    public bool IsActive { get; set; } = true;
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

    //Constructor for EF
    public Genre()
    {
        Name = string.Empty;
    }

    //Constructor with isSubgenre parameter
    public Genre(bool isSubgenre) : this()
    {
        IsSubgenre = isSubgenre;
        Id = GenerateGenreId(isSubgenre);
    }

    //Generate ID  G- + 12 characters
    private static string GenerateGenreId(bool isSubgenre)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        
        var genreId = new string(Enumerable.Repeat(chars, 12)
            .Select(s => s[random.Next(s.Length)]).ToArray());

        if (!isSubgenre) return $"G-{genreId}";
        {
            //For subgenres: G-{12char}S-{12char}
            var subgenreId = new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return $"G-{genreId}S-{subgenreId}";
        }

        //For genres: G-{12chars}
    }

    // Method to set ID after determining if it's a subgenre
    public void SetId()
    {
        if (string.IsNullOrEmpty(Id))
        {
            Id = GenerateGenreId(IsSubgenre);
        }
    }
}