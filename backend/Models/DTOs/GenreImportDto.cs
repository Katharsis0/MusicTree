// Models/DTOs/GenreImportDto.cs
using System.ComponentModel.DataAnnotations;

namespace MusicTree.Models.DTOs
{
    public class GenreImportDto : IValidatableObject
    {
        [Required]
        [StringLength(30, MinimumLength = 3)]
        public string name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? description { get; set; }

        public bool active { get; set; } = true;

        // Color as hex string (for JSON compatibility)
        public string? rgb { get; set; }

        public int? creation_year { get; set; }

        public string? origin_country { get; set; }

        [Required]
        [Range(0, 1)]
        public float mode { get; set; }

        [Required]
        public BpmRangeDto bpm { get; set; } = new();

        [Range(-1, 11)]
        public int tipical_mode { get; set; } = -1;

        [Required]
        [Range(-60, 0)]
        public int volume { get; set; }

        [Required]
        [Range(0, 8)]
        public int compas { get; set; }

        [Required]
        [Range(0, 3600)]
        public int avrg_duration { get; set; }

        public bool is_subgenre { get; set; } = false;

        public string? parent_genre { get; set; }

        public List<GenreRelationImportDto>? related_genre { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Validate RGB color format if provided
            if (!string.IsNullOrEmpty(rgb) && !IsValidRgbColor(rgb))
            {
                yield return new ValidationResult(
                    "Color must be in valid RGB format (rgb(r,g,b)) where r,g,b are 0-255",
                    new[] { nameof(rgb) });
            }

            // Subgenre validation
            if (is_subgenre && string.IsNullOrEmpty(parent_genre))
            {
                yield return new ValidationResult(
                    "Parent genre is required for subgenres",
                    new[] { nameof(parent_genre) });
            }

            // Subgenre cannot have color
            if (is_subgenre && !string.IsNullOrEmpty(rgb))
            {
                yield return new ValidationResult(
                    "Subgenres cannot have a color assigned",
                    new[] { nameof(rgb) });
            }

            // BPM validation
            if (bpm.min > bpm.max)
            {
                yield return new ValidationResult(
                    "BPM minimum cannot be greater than maximum",
                    new[] { nameof(bpm) });
            }
        }

        private static bool IsValidRgbColor(string color)
        {
            if (string.IsNullOrEmpty(color))
                return false;

            //Check if format is rgb(r,g,b)
            if (!color.StartsWith("rgb(") || !color.EndsWith(")"))
                return false;

            // Extract RGB values
            var rgbContent = color[4..^1]; 
            var parts = rgbContent.Split(',');

            if (parts.Length != 3)
                return false;

            // Validate each component is a number between 0-255
            foreach (var part in parts)
            {
                if (!int.TryParse(part.Trim(), out int value) || value < 0 || value > 255)
                    return false;
            }

            return true;
        }
    }
    
    public class BpmRangeDto
    {
        [Required]
        [Range(0, 250)]
        public int min { get; set; }

        [Required]
        [Range(0, 250)]
        public int max { get; set; }
    }
    public class GenreRelationImportDto
    {
        [Required]
        public string name { get; set; } = string.Empty;

        [Required]
        [Range(1, 10)]
        public int influence { get; set; } = 5;
    }
}
