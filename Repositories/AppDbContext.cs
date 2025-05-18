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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships
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

            // Configure many-to-many relationship for RelatedGenres
            modelBuilder.Entity<Genre>()
                .HasMany(g => g.RelatedGenres)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "GenreRelations",
                    j => j.HasOne<Genre>().WithMany().HasForeignKey("RelatedGenreId"),
                    j => j.HasOne<Genre>().WithMany().HasForeignKey("GenreId"));
        }
    }
}