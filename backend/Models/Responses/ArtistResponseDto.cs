namespace MusicTree.Models.Responses
{
    public class ArtistResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Biography { get; set; }
        public string OriginCountry { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ArtistDetailResponseDto : ArtistResponseDto
    {
        public List<GenreBasicDto> AssociatedGenres { get; set; } = new();
        public List<GenreBasicDto> AssociatedSubgenres { get; set; } = new();
        public ArtistStatisticsDto Statistics { get; set; } = new();
    }

    public class ArtistListItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string OriginCountry { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int GenreCount { get; set; }
    }

    public class ArtistStatisticsDto
    {
        public int GenreCount { get; set; }
        public int SubgenreCount { get; set; }
        public double DiversityScore { get; set; }
        public string? MostInfluentialGenre { get; set; }
    }

    public class GenreBasicDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsSubgenre { get; set; }
        public string? ParentGenreName { get; set; }
        public float InfluenceCoefficient { get; set; }
    }

    public class PagedResponse<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
    }
}