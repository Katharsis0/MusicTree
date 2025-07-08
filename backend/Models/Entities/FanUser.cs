using System.ComponentModel.DataAnnotations;

public class FanUser
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Nickname { get; set; }

    [Required]
    public string Password { get; set; }

    [Required]
    public string Nombre { get; set; }

    public string Pais { get; set; }
    public string Avatar { get; set; }

    public string Rol { get; set; } = "fanatico";

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public List<FanUserGenero> GenerosFavoritos { get; set; } = new();
}
