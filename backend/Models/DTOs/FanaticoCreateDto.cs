// Models/DTOs/ClusterCreateDto.cs
using System.ComponentModel.DataAnnotations;
using MusicTree.Models.Entities;
using System.Text.Json.Serialization;
namespace MusicTree.Models.DTOs;


public class FanaticoCreateDto
{
    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("pais")]
    public string Country { get; set; }

    [JsonPropertyName("avatar")]
    public string Avatar { get; set; }

    [JsonPropertyName("generosFavoritos")]
    public List<string> GenerosFavoritos { get; set; } = new();
    
    public class FanaticoGenreRelationDto
    {
        [Required]
        public string GenreId { get; set; } = string.Empty;

        [Range(0.1f, 10.0f)]
        public float InfluenceCoefficient { get; set; } = 1.0f;
    }
}
