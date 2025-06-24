using System.Text.Json.Serialization;

public class FanaticoRegisterDto
{
    [JsonPropertyName("NombreUsuario")]
    public string NombreUsuario { get; set; }

    [JsonPropertyName("Contrasena")]
    public string Contrasena { get; set; }

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; }

    [JsonPropertyName("pais")]
    public string Pais { get; set; }

    [JsonPropertyName("avatar")]
    public string Avatar { get; set; }
}