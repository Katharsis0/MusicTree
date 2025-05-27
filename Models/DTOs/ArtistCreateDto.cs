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
    }
}