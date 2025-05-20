using System.ComponentModel.DataAnnotations;

namespace MusicTree.Models.Entities
{
    public class GenreRelation
    {
        [Key]
        public string GenreId { get; set; } = string.Empty;

        [Key]
        public string RelatedGenreId { get; set; } = string.Empty;

        [Range(1, 10)]
        public int Influence { get; set; } = 5;

        [Range(0, 1)]
        public float MGPC { get; set; } = 0;

        // Navigation properties
        public Genre Genre { get; set; } = null!;
        public Genre RelatedGenre { get; set; } = null!;
    }
}