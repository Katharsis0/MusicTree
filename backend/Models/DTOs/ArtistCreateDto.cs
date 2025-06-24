using System.ComponentModel.DataAnnotations;

namespace MusicTree.Models.DTOs
{
    public class ArtistCreateDto : IValidatableObject
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Biography { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string OriginCountry { get; set; } = string.Empty;

        [Required]
        public string ActivityYears { get; set; } = string.Empty;

        // Cover image (will be handled separately for file upload)
        public IFormFile? CoverImage { get; set; }

        // Genre associations (required - at least one)
        [Required]
        public List<ArtistGenreRelationDto> ArtistRelatedGenres { get; set; } = new();

        // Subgenre associations (optional)
        public List<ArtistSubgenreRelationDto>? ArtistRelatedSubgenres { get; set; }

        // Band members (optional but recommended)
        public List<ArtistMemberDto>? Members { get; set; }

        // Discography (optional)
        public List<AlbumDto>? Albums { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Validate activity years format
            if (!IsValidActivityYearsFormat(ActivityYears))
            {
                yield return new ValidationResult(
                    "Activity years must be in valid format (e.g., '1965–1994', '2005', '2013–presente')",
                    new[] { nameof(ActivityYears) });
            }

            // At least one genre is required
            if (ArtistRelatedGenres == null || !ArtistRelatedGenres.Any())
            {
                yield return new ValidationResult(
                    "At least one genre association is required",
                    new[] { nameof(ArtistRelatedGenres) });
            }

            // Validate cover image if provided
            if (CoverImage != null)
            {
                var imageValidation = ValidateImage(CoverImage);
                if (!string.IsNullOrEmpty(imageValidation))
                {
                    yield return new ValidationResult(imageValidation, new[] { nameof(CoverImage) });
                }
            }

            // Validate album cover images if provided
            if (Albums != null)
            {
                foreach (var album in Albums.Where(a => a.CoverImage != null))
                {
                    var imageValidation = ValidateImage(album.CoverImage!);
                    if (!string.IsNullOrEmpty(imageValidation))
                    {
                        yield return new ValidationResult(
                            $"Album '{album.Title}' cover image: {imageValidation}",
                            new[] { nameof(Albums) });
                    }
                }
            }
        }

        private static bool IsValidActivityYearsFormat(string activityYears)
        {
            if (string.IsNullOrWhiteSpace(activityYears))
                return false;

            // Split by commas and validate each part
            var parts = activityYears.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                
                // Check for single year (e.g., "2005")
                if (int.TryParse(trimmed, out int year) && year >= 1900 && year <= DateTime.Now.Year + 1)
                    continue;

                // Check for range (e.g., "1965–1994" or "2013–presente")
                if (trimmed.Contains('-'))
                {
                    var rangeParts = trimmed.Split('-');
                    if (rangeParts.Length == 2)
                    {
                        var start = rangeParts[0].Trim();
                        var end = rangeParts[1].Trim();

                        if (int.TryParse(start, out int startYear) && startYear >= 1900)
                        {
                            if (end.Equals("presente", StringComparison.OrdinalIgnoreCase) ||
                                end.Equals("present", StringComparison.OrdinalIgnoreCase))
                                continue;

                            if (int.TryParse(end, out int endYear) && endYear >= startYear && endYear <= DateTime.Now.Year + 1)
                                continue;
                        }
                    }
                }

                return false; // Invalid part found
            }

            return true;
        }

        private static string? ValidateImage(IFormFile image)
        {
            const int maxSizeBytes = 5 * 1024 * 1024; // 5MB
            const int minWidth = 800;
            const int minHeight = 800;

            if (image.Length > maxSizeBytes)
                return "Image size must not exceed 5MB";

            var allowedTypes = new[] { "image/jpeg", "image/jpg" };
            if (!allowedTypes.Contains(image.ContentType.ToLower()))
                return "Only JPEG images are allowed";

            var allowedExtensions = new[] { ".jpg", ".jpeg" };
            var extension = Path.GetExtension(image.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return "File must have .jpg or .jpeg extension";

            // Note: Image dimension validation would require additional libraries
            // For now, we'll validate other constraints
            return null;
        }

        public class ArtistGenreRelationDto
        {
            [Required]
            public string GenreId { get; set; } = string.Empty;

            [Range(0.1f, 10.0f)]
            public float InfluenceCoefficient { get; set; } = 1.0f;
        }

        public class ArtistSubgenreRelationDto
        {
            [Required]
            public string GenreId { get; set; } = string.Empty;

            [Range(0.1f, 10.0f)]
            public float InfluenceCoefficient { get; set; } = 1.0f;
        }

        public class ArtistMemberDto
        {
            [Required]
            [StringLength(100, MinimumLength = 3)]
            public string FullName { get; set; } = string.Empty;

            [Required]
            [StringLength(100)]
            public string Instrument { get; set; } = string.Empty;

            [Required]
            public string ActivityPeriod { get; set; } = string.Empty;

            public bool IsActive { get; set; } = true;
        }

        public class AlbumDto
        {
            [Required]
            [StringLength(200, MinimumLength = 1)]
            public string Title { get; set; } = string.Empty;

            [Required]
            public DateTime ReleaseDate { get; set; }

            public IFormFile? CoverImage { get; set; }

            [Range(1, int.MaxValue)]
            public int DurationSeconds { get; set; }
        }
    }
}