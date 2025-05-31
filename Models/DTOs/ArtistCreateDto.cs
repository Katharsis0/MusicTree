//Models/DTOs/ArtistCreateDto.cs
using System.ComponentModel.DataAnnotations;

//Objeto de data exchange en el API  
namespace MusicTree.Models.DTOs
{
    public class ArtistCreateDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Biography { get; set; }

        public string OriginCountry { get; set; }

        public List<ArtistGenreRelationDto> ArtistRelatedGenres { get; set; }
        public List<ArtistSubgenreRelationDto>? ArtistRelatedSubgenres { get; set; }

        public class ArtistGenreRelationDto
        {
            [Required]
            public string GenreId { get; set; } = string.Empty;  // ID of the genre
        }

        public class ArtistSubgenreRelationDto
        {
            [Required]
            public string GenreId { get; set; } = string.Empty;  // ID of the genre
        }
    }
}