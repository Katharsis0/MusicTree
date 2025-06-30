// Models/DTOs/ClusterCreateDto.cs
using System.ComponentModel.DataAnnotations;
using MusicTree.Models.Entities;
using System.Text.Json.Serialization;

namespace MusicTree.Models.DTOs
{

    public class FanaticoCalificarDto
    {
        [JsonPropertyName("username")] public string Username { get; set; }

        [JsonPropertyName("artistID")] public string ArtistID { get; set; }

        [JsonPropertyName("calificacion")] public int Calificacion { get; set; }
    }
}