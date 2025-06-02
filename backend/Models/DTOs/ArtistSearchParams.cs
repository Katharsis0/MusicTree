using System.ComponentModel.DataAnnotations;

namespace MusicTree.Models.DTOs
{
    public class ArtistSearchParams
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(100)]
        public string? OriginCountry { get; set; }

        public bool IncludeInactive { get; set; } = false;

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 20;

        public string SortBy { get; set; } = "Name";
        public string SortDirection { get; set; } = "asc"; // "asc" or "desc"

        public string? GenreId { get; set; }
    }
}