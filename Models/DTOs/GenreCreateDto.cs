//Models/DTOs/GenreCreateDto.cs
using System.ComponentModel.DataAnnotations;

namespace MusicTree.Models.DTOs
{
    public class GenreCreateDto
    {
        [Required]
        [StringLength(30, MinimumLength = 3)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        // Cluster association (optional, but cannot be set if IsSubgenre=true)
        public string? ClusterId { get; set; }

        // Subgenre hierarchy
        public bool IsSubgenre { get; set; }
        public string? ParentGenreId { get; set; }  // Required if IsSubgenre=true

        // Musical attributes (validated per user story rules)
        [Range(0, 1)]
        public float GenreTipicalMode { get; set; }  // 0=minor, 1=major

        [Range(0, 250)]
        public int Bpm { get; set; }  // Beats per minute

        [Range(-60, 0)]
        public int Volume { get; set; }  // In decibels (dB)

        [Range(0, 8)]
        public int CompasMetric { get; set; }  // 0=undefined, 2=2/4, 3=3/4, etc.

        [Range(0, 3600)]
        public int AvrgDuration { get; set; }  // In seconds

        // Optional fields
        public string? Color { get; set; }  // Disabled if IsSubgenre=true
        public string? GenreCreationYear { get; set; }
        public string? GenreOriginCountry { get; set; }  // Country list (e.g., UN countries)

        // Related genres (for MGPC calculations)
        public List<GenreRelationDto>? RelatedGenres { get; set; }
    }

    // Helper DTO for genre relationships (influences + MGPC)
    public class GenreRelationDto
    {
        [Required]
        public string GenreId { get; set; }  // ID of the influencing genre

        [Range(1, 10)]
        public int InfluenceStrength { get; set; } = 5;  // Default=5 (per user story)
    }
}