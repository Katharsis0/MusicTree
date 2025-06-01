// Models/DTOs/GenreImportDto.cs
using System.ComponentModel.DataAnnotations;

namespace MusicTree.Models.DTOs
{
    public class GenreImportDto : IValidatableObject
    {
        [Required]
        [StringLength(30, MinimumLength = 3)]
        public string nombre { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? descripcion { get; set; }

        public bool activo { get; set; } = true;

        // Color as hex string (for JSON compatibility)
        public string? color { get; set; }

        public int? anio_creacion { get; set; }

        public string? pais_origen { get; set; }

        [Required]
        [Range(0, 1)]
        public float modo { get; set; }

        [Required]
        public BpmRangeDto bpm { get; set; } = new();

        [Range(-1, 11)]
        public int tono_dominante { get; set; } = -1;

        [Required]
        [Range(-60, 0)]
        public int volumen_tipico_db { get; set; }

        [Required]
        [Range(0, 8)]
        public int compas { get; set; }

        [Required]
        [Range(0, 3600)]
        public int duracion_promedio_segundos { get; set; }

        public bool es_subgenero { get; set; } = false;

        public string? genero_padre { get; set; }

        public List<GenreRelationImportDto>? generos_relacionados { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Validate RGB color format if provided
            if (!string.IsNullOrEmpty(color) && !IsValidRgbColor(color))
            {
                yield return new ValidationResult(
                    "Color must be in valid RGB format (rgb(r,g,b)) where r,g,b are 0-255",
                    new[] { nameof(color) });
            }

            // Subgenre validation
            if (es_subgenero && string.IsNullOrEmpty(genero_padre))
            {
                yield return new ValidationResult(
                    "Género padre is required for subgenres",
                    new[] { nameof(genero_padre) });
            }

            // Subgenre cannot have color
            if (es_subgenero && !string.IsNullOrEmpty(color))
            {
                yield return new ValidationResult(
                    "Subgenres cannot have a color assigned",
                    new[] { nameof(color) });
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
        public string nombre { get; set; } = string.Empty;

        [Required]
        [Range(1, 10)]
        public int influencia { get; set; } = 5;
    }
}
