using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicTree.Models.Entities
{
    public class GenreRelation
    {
        [Key, Column(Order = 0)]
        public string GenreId { get; set; }  // Changed from int to string

        [ForeignKey("GenreId")]
        public Genre Genre { get; set; }

        [Key, Column(Order = 1)]
        public string RelatedGenreId { get; set; }  // Changed from int to string

        [ForeignKey("RelatedGenreId")]
        public Genre RelatedGenre { get; set; }

        [Range(1, 10)]
        public int Influence { get; set; } = 5;

        public float MGPC { get; set; } 
    }
}
