using System.ComponentModel.DataAnnotations;

namespace MusicTree.Models.DTOs;

public class FanaticoLoginDto
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    public string NombreUsuario { get; set; }

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Contrasena { get; set; }
}