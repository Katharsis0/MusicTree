using System.ComponentModel.DataAnnotations;

namespace MusicTree.Models.Entities;


//Representación  del objeto en la DB
public class Fanatico{
    [Key]
    public string Username { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }
    public string Country { get; set; }
    public string Avatar { get; set; }
    //public List<Genre> Genres { get; set; } = new List<Genre>();
}