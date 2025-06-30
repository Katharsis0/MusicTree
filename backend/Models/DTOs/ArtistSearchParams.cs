using System.ComponentModel.DataAnnotations;

namespace MusicTree.Models.DTOs
{
    public class ArtistSearchParams
    {
        // Basic text searches
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(100)]
        public string? OriginCountry { get; set; }

        public string? ActivityYears { get; set; }

        // Include inactive artists
        public bool IncludeInactive { get; set; } = false;

        // Pagination
        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 20;

        // Sorting
        public string SortBy { get; set; } = "Name";
        public string SortDirection { get; set; } = "asc"; // "asc" or "desc"

        // Genre filtering
        public string? GenreId { get; set; }
        public string? SubgenreId { get; set; }
        
        // Multiple genre filtering
        public List<string>? GenreIds { get; set; }
        public List<string>? SubgenreIds { get; set; }
        public string GenreLogic { get; set; } = "OR"; // "AND" or "OR" 

        // Album filtering
        public bool? HasAlbums { get; set; }
        public int? MinAlbumCount { get; set; }
        public int? MaxAlbumCount { get; set; }

        // Member filtering
        public int? MinMemberCount { get; set; }
        public int? MaxMemberCount { get; set; }
        public bool? HasActiveMembers { get; set; }

        // Date filtering
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }

        // NEW: Activity year range filtering
        public int? ActiveFromYear { get; set; }
        public int? ActiveToYear { get; set; }
        public bool? CurrentlyActive { get; set; } // Still active (activity years contains "presente")

        //Cluster filtering (indirect through genres)
        public string? ClusterId { get; set; }

      
        // Advanced text search
        public bool ExactNameMatch { get; set; } = false;
        public bool CaseSensitive { get; set; } = false;

        // Statistics filtering
        public int? MinGenreCount { get; set; }
        public int? MaxGenreCount { get; set; }
        public int? MinCommentCount { get; set; }
        public int? MinPhotoCount { get; set; }
        public int? MinEventCount { get; set; }
    }
}