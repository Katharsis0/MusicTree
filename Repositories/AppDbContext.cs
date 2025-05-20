using Microsoft.EntityFrameworkCore;
using MusicTree.Models.Entities;

namespace MusicTree.Repositories
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Cluster> Clusters { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<GenreRelation> GenreRelations { get; set; } // Added missing DbSet

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Set relationships
            modelBuilder.Entity<Genre>()
                .HasOne(g => g.ParentGenre)
                .WithMany()
                .HasForeignKey(g => g.ParentGenreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Genre>()
                .HasOne(g => g.Cluster)
                .WithMany(c => c.Genres)
                .HasForeignKey(g => g.ClusterId)
                .OnDelete(DeleteBehavior.SetNull);

            //Set many-to-many relationship using GenreRelation entity
            modelBuilder.Entity<GenreRelation>()
                .HasKey(gr => new { gr.GenreId, gr.RelatedGenreId });

            modelBuilder.Entity<GenreRelation>()
                .HasOne(gr => gr.Genre)
                .WithMany(g => g.RelatedGenresAsSource)
                .HasForeignKey(gr => gr.GenreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GenreRelation>()
                .HasOne(gr => gr.RelatedGenre)
                .WithMany(g => g.RelatedGenresAsTarget)
                .HasForeignKey(gr => gr.RelatedGenreId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}