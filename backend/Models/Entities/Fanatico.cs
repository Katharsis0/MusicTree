using System.ComponentModel.DataAnnotations;

namespace MusicTree.Models.Entities
{
//Representación  del objeto en la DB
    public class Fanatico
    {
        [Key] public string Username { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Country { get; set; }

        public string Avatar { get; set; }
        //public List<Genre> Genres { get; set; } = new List<Genre>();
    }

// Artist-Genre relationship (for main genres)
    public class FanaticoGenre
    {
        public string ArtistId { get; set; } = string.Empty;
        public string GenreId { get; set; } = string.Empty;
        public float InfluenceCoefficient { get; set; } = 1.0f;
        public DateTime AssociatedDate { get; set; } = DateTime.UtcNow;

        public Artist Artist { get; set; } = null!;
        public Genre Genre { get; set; } = null!;
    }

}