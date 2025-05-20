// Models/DTOs/ClusterCreateDto.cs
using System.ComponentModel.DataAnnotations;

namespace MusicTree.Models.DTOs;
//Data Transfer Object de Requests

public class ClusterCreateDto
{
    [Required]
    [StringLength(30, MinimumLength = 3)]
    public string Name { get; set; }

    [StringLength(300)]
    public string? Description { get; set; }  //Opcional
}