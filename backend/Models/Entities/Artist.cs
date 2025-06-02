using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicTree.Models.Entities
{
    public class Artist
    {
        [Key]
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Biography { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string OriginCountry { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

        //Navigation relationships
        public ICollection<ArtistGenre> ArtistGenres { get; set; } = new List<ArtistGenre>();
        public ICollection<ArtistSubgenre> ArtistSubgenres { get; set; } = new List<ArtistSubgenre>();

        //Computed properties
        [NotMapped]
        public IEnumerable<Genre> AllAssociatedGenres => 
            ArtistGenres.Select(ag => ag.Genre)
            .Concat(ArtistSubgenres.Select(ag => ag.Genre));

        [NotMapped]
        public int GenreCount => ArtistGenres.Count + ArtistSubgenres.Count;

        //Constructor
        public Artist()
        {
            Id = GenerateArtistId();
        }

        private static string GenerateArtistId()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var randomId = new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return $"A-{randomId}";
        }
    }

    //Intersección de tablas para relación de genre
    public class ArtistGenre
    {
        public string ArtistId { get; set; } = string.Empty;
        public string GenreId { get; set; } = string.Empty;
        public float InfluenceCoefficient { get; set; } = 1.0f;
        public DateTime AssociatedDate { get; set; } = DateTime.UtcNow;

        public Artist Artist { get; set; } = null!;
        public Genre Genre { get; set; } = null!;
    }

    public class ArtistSubgenre
    {
        public string ArtistId { get; set; } = string.Empty;
        public string GenreId { get; set; } = string.Empty;
        public float InfluenceCoefficient { get; set; } = 1.0f;
        public DateTime AssociatedDate { get; set; } = DateTime.UtcNow;

        public Artist Artist { get; set; } = null!;
        public Genre Genre { get; set; } = null!;
    }
}