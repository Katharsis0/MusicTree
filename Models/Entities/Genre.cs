using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace MusicTree.Models.Entities;

//Representación  del objeto en la DB 
public class Genre
{
    [Key] 
    public string Id { get; set; } = GenerateGenreId();

    [Required]
    [StringLength(30, MinimumLength = 3)]
    public string Name { get; set; }

    [StringLength(1000)] public string? Description { get; set; }
    public bool IsSubgenre { get; set; }

    // Navigation properties
    public string? ParentGenreId { get; set; }
    public Genre? ParentGenre { get; set; }

    public string? ClusterId { get; set; }
    public Cluster? Cluster { get; set; }
    [Range(-1, 11)] public int Key { get; set; } = -1; //Nota dominante

    //Rango de Beats per minute
    public int BpmLower { get; set; }
    public int BpmUpper { get; set; }

    //Generos relacionados
    public ICollection<GenreRelation> RelatedGenresAsSource { get; set; } = new List<GenreRelation>();
    public ICollection<GenreRelation> RelatedGenresAsTarget { get; set; } = new List<GenreRelation>();

    //Obtener todos los generos relacionados (no va a DB)
    [NotMapped]
    public IEnumerable<Genre> RelatedGenres =>
        RelatedGenresAsSource.Select(r => r.RelatedGenre)
            .Concat(RelatedGenresAsTarget.Select(r => r.Genre));

    //Musical attributes
    public float GenreTipicalMode { get; set; }
    public int Bpm { get; set; }
    public int Volume { get; set; }
    public int CompasMetric { get; set; }
    public int AvrgDuration { get; set; }

    //Optional
    public string? Color { get; set; }
    public int? GenreCreationYear { get; set; }
    public string? GenreOriginCountry { get; set; }

    //Metadata
    public bool IsActive { get; set; } = true;
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;


    private static string GenerateGenreId()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return $"G-{new string(Enumerable.Repeat(chars, 12)
            .Select(s => s[random.Next(s.Length)]).ToArray())}";
    }
}