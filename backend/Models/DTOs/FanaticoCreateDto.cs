// Models/DTOs/ClusterCreateDto.cs
using System.ComponentModel.DataAnnotations;
using MusicTree.Models.Entities;

namespace MusicTree.Models.DTOs;

public class FanaticoCreateDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; }

    [Required]
    [StringLength(100)]
    public string Username { get; set; }
    
    [Required]
    [StringLength(300)]
    public string Password { get; set; }  //Opcional
    
    [Required]
    public string Country { get; set; }  //Opcional
    
    [Required]
    public string Avatar { get; set; }  //Opcional
    
    //[Required]
    //public List<Genre> FanaticoRelatedGenres { get; set; } = new();
    
    
    public class FanaticoGenreRelationDto
    {
        [Required]
        public string GenreId { get; set; } = string.Empty;

        [Range(0.1f, 10.0f)]
        public float InfluenceCoefficient { get; set; } = 1.0f;
    }
}
