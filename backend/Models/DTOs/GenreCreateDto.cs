//Models/DTOs/GenreCreateDto.cs
using System.ComponentModel.DataAnnotations;

//Objeto de data exchange en el API  
namespace MusicTree.Models.DTOs
{
    public class GenreCreateDto : IValidatableObject
    {
        [Required]
        [StringLength(30, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        //Cluster association (optional, but cannot be set if IsSubgenre=true)
        public string? ClusterId { get; set; }

        //Subgenre hierarchy
        public bool IsSubgenre { get; set; }
        public string? ParentGenreId { get; set; }  //Needed if IsSubgenre=true

        //Musical attributes 
        [Required]
        [Range(0, 1)]
        public float GenreTipicalMode { get; set; }  // 0=minor, 1=major

        [Required]
        [Range(-60, 0)]
        public int Volume { get; set; }  // In decibels (dB)

        [Required]
        [Range(0, 8)]
        public int CompasMetric { get; set; }  // 0=undefined, 2=2/4, 3=3/4, etc.

        [Required]
        [Range(0, 3600)]
        public int AvrgDuration { get; set; }  // In seconds

        [Range(-1, 11)]
        public int Key { get; set; } = -1; // Musical key (0=C, 1=C#/Db, etc., -1=undefined)

        //BPM Range fields 
        [Required]
        [Range(0, 250)]
        public int BpmLower { get; set; }

        [Required]
        [Range(0, 250)] 
        public int BpmUpper { get; set; }
        
        //RGB Color fields (instead of hex)
        [Range(0, 255)]
        public int? ColorR { get; set; }  // Red component (0-255)
        
        [Range(0, 255)]
        public int? ColorG { get; set; }  // Green component (0-255)
        
        [Range(0, 255)]
        public int? ColorB { get; set; }  // Blue component (0-255)
        
        // Optional fields
        public int? GenreCreationYear { get; set; }
        public string? GenreOriginCountry { get; set; }  //Country list (UN countries)

        //Related genres (for MGPC calculations and influence)
        public List<GenreRelationDto>? RelatedGenres { get; set; }
        
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //BPM validation
            if (BpmLower > BpmUpper)
            {
                yield return new ValidationResult(
                    "BPM lower bound cannot be greater than upper bound",
                    new[] { nameof(BpmLower), nameof(BpmUpper) });
            }

            //Subgenre validation
            if (IsSubgenre && string.IsNullOrEmpty(ParentGenreId))
            {
                yield return new ValidationResult(
                    "Parent genre is required for subgenres",
                    new[] { nameof(ParentGenreId) });
            }

            //Subgenre cannot have cluster
            if (IsSubgenre && !string.IsNullOrEmpty(ClusterId))
            {
                yield return new ValidationResult(
                    "Subgenres cannot be associated with clusters directly",
                    new[] { nameof(ClusterId) });
            }

            //Subgenre cannot have color
            if (IsSubgenre && (ColorR.HasValue || ColorG.HasValue || ColorB.HasValue))
            {
                yield return new ValidationResult(
                    "Subgenres cannot have a color assigned",
                    new[] { nameof(ColorR), nameof(ColorG), nameof(ColorB) });
            }

            //RGB color validation - all components must be provided together
            var colorComponentsProvided = new[] { ColorR.HasValue, ColorG.HasValue, ColorB.HasValue };
            if (colorComponentsProvided.Any(x => x) && !colorComponentsProvided.All(x => x))
            {
                yield return new ValidationResult(
                    "If color is specified, all RGB components (R, G, B) must be provided",
                    new[] { nameof(ColorR), nameof(ColorG), nameof(ColorB) });
            }
        }
    }

    // Helper DTO for genre relationships (influences + MGPC)
    public class GenreRelationDto
    {
        [Required]
        public string GenreId { get; set; } = string.Empty;  // ID of the influencing genre

        [Required]
        [Range(1, 10)]
        public int InfluenceStrength { get; set; } = 5;  // Influence strength (1-10 scale)
    }
}