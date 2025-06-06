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

        [Required]
        [StringLength(200)]
        public string ActivityYears { get; set; } = string.Empty; // e.g., "1965–1994, 2005, 2013–presente"

        [StringLength(500)]
        public string? CoverImageUrl { get; set; } // Path to cover image

        public bool IsActive { get; set; } = true;
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<ArtistGenre> ArtistGenres { get; set; } = new List<ArtistGenre>();
        public ICollection<ArtistSubgenre> ArtistSubgenres { get; set; } = new List<ArtistSubgenre>();
        public ICollection<ArtistMember> Members { get; set; } = new List<ArtistMember>();
        public ICollection<Album> Albums { get; set; } = new List<Album>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Photo> PhotoGallery { get; set; } = new List<Photo>();
        public ICollection<Event> Events { get; set; } = new List<Event>();

        // Computed properties
        [NotMapped]
        public IEnumerable<Genre> AllAssociatedGenres => 
            ArtistGenres.Select(ag => ag.Genre)
            .Concat(ArtistSubgenres.Select(ag => ag.Genre));

        [NotMapped]
        public int GenreCount => ArtistGenres.Count + ArtistSubgenres.Count;

        [NotMapped]
        public int AlbumCount => Albums.Count;

        [NotMapped]
        public int ActiveMemberCount => Members.Count(m => m.IsActive);

        // Constructor
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

    // Artist-Genre relationship (for main genres)
    public class ArtistGenre
    {
        public string ArtistId { get; set; } = string.Empty;
        public string GenreId { get; set; } = string.Empty;
        public float InfluenceCoefficient { get; set; } = 1.0f;
        public DateTime AssociatedDate { get; set; } = DateTime.UtcNow;

        public Artist Artist { get; set; } = null!;
        public Genre Genre { get; set; } = null!;
    }

    // Artist-Subgenre relationship
    public class ArtistSubgenre
    {
        public string ArtistId { get; set; } = string.Empty;
        public string GenreId { get; set; } = string.Empty;
        public float InfluenceCoefficient { get; set; } = 1.0f;
        public DateTime AssociatedDate { get; set; } = DateTime.UtcNow;

        public Artist Artist { get; set; } = null!;
        public Genre Genre { get; set; } = null!;
    }

    // Artist members (band members)
    public class ArtistMember
    {
        [Key]
        public string Id { get; set; } = string.Empty;

        [Required]
        public string ArtistId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Instrument { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string ActivityPeriod { get; set; } = string.Empty; // e.g., "1965–1994, 2005–presente"

        public bool IsActive { get; set; } = true;
        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;

        public Artist Artist { get; set; } = null!;

        public ArtistMember()
        {
            Id = GenerateMemberId();
        }

        private static string GenerateMemberId()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var randomId = new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return $"M-{randomId}";
        }
    }

    // Albums/Discography
    public class Album
    {
        [Key]
        public string Id { get; set; } = string.Empty;

        [Required]
        public string ArtistId { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public DateTime ReleaseDate { get; set; }

        [StringLength(500)]
        public string? CoverImageUrl { get; set; }

        [Range(0, int.MaxValue)]
        public int DurationSeconds { get; set; } // Total duration in seconds

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Artist Artist { get; set; } = null!;

        // Computed properties
        [NotMapped]
        public TimeSpan Duration => TimeSpan.FromSeconds(DurationSeconds);

        [NotMapped]
        public string FormattedDuration => 
            DurationSeconds < 3600 
                ? $"{DurationSeconds / 60}:{DurationSeconds % 60:D2}"
                : $"{DurationSeconds / 3600}:{(DurationSeconds % 3600) / 60:D2}:{DurationSeconds % 60:D2}";

        public Album()
        {
            // ID will be set when ArtistId is available
        }

        public void SetId(string artistId)
        {
            if (string.IsNullOrEmpty(Id))
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                var random = new Random();
                var randomId = new string(Enumerable.Repeat(chars, 12)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
                Id = $"{artistId}-D-{randomId}";
            }
        }
    }

    // Comments thread
    public class Comment
    {
        [Key]
        public string Id { get; set; } = string.Empty;

        [Required]
        public string ArtistId { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string AuthorName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public Artist Artist { get; set; } = null!;

        public Comment()
        {
            Id = GenerateCommentId();
        }

        private static string GenerateCommentId()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var randomId = new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return $"C-{randomId}";
        }
    }

    // Photo gallery
    public class Photo
    {
        [Key]
        public string Id { get; set; } = string.Empty;

        [Required]
        public string ArtistId { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Caption { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public Artist Artist { get; set; } = null!;

        public Photo()
        {
            Id = GeneratePhotoId();
        }

        private static string GeneratePhotoId()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var randomId = new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return $"P-{randomId}";
        }
    }

    // Events calendar
    public class Event
    {
        [Key]
        public string Id { get; set; } = string.Empty;

        [Required]
        public string ArtistId { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(200)]
        public string Venue { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public Artist Artist { get; set; } = null!;

        public Event()
        {
            Id = GenerateEventId();
        }

        private static string GenerateEventId()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var randomId = new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return $"E-{randomId}";
        }
    }
}