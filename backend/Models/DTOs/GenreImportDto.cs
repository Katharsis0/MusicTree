// Models/DTOs/GenreImportDto.cs
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace MusicTree.Models.DTOs
{
    public class GenreImportDto : IValidatableObject
    {
        [Required]
        [StringLength(30, MinimumLength = 3)]
        [JsonPropertyName("nombre")]
        public string name { get; set; } = string.Empty;

        [StringLength(1000)]
        [JsonPropertyName("descripcion")]
        public string? description { get; set; }
        [JsonPropertyName("activo")]
        public bool active { get; set; } = true;

        [JsonPropertyName("color")]
        public string? rgb { get; set; }
        [JsonPropertyName("anio_creacion")]
        public int? creation_year { get; set; }
        [JsonPropertyName("pais_origen")]
        public string? origin_country { get; set; }
        [JsonPropertyName("modo")]
        [Required]
        [Range(0, 1)]
        public float mode { get; set; }
        [JsonPropertyName("bpm")]
        [Required]
        public BpmRangeDto bpm { get; set; } = new();
        [JsonPropertyName("tono_dominante")]
        [Range(-1, 11)]
        public int tipical_mode { get; set; } = -1;
        [JsonPropertyName("volumen_tipico_db")]
        [Required]
        [Range(-60, 0)]
        public int volume { get; set; }
        [JsonPropertyName("compas")]
        [Required]
        [Range(0, 8)]
        public int compas { get; set; }
        [JsonPropertyName("duracion_promedio_segundos")]
        [Required]
        [Range(0, 3600)]
        public int avrg_duration { get; set; }
        [JsonPropertyName("es_subgenero")]
        public bool is_subgenre { get; set; } = false;
        [JsonPropertyName("genero_padre")]
        public string? parent_genre { get; set; }
        [JsonPropertyName("generos_relacionados")]
        public List<GenreRelationImportDto>? related_genre { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {

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
        [JsonPropertyName("nombre")]
        public string name { get; set; } = string.Empty;

        [Required]
        [Range(1, 10)]
        [JsonPropertyName("influencia")]
        public int influence { get; set; } = 5;
    }
}
