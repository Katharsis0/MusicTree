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
        public DbSet<GenreRelation> GenreRelations { get; set; }
        public DbSet<Artist> Artists { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Cluster 
            modelBuilder.Entity<Cluster>(entity =>
            {
                entity.Property(c => c.Id).HasMaxLength(128);
                entity.Property(c => c.Name).HasMaxLength(30).IsRequired();
                entity.Property(c => c.Description).HasMaxLength(300);
                entity.Property(c => c.IsActive).HasDefaultValue(true);
                entity.Property(c => c.TimeStamp).HasDefaultValueSql("NOW()");
            });

            // Configure Genre 
            modelBuilder.Entity<Genre>(entity =>
            {
                entity.Property(g => g.Id).HasMaxLength(128);
                entity.Property(g => g.Name).HasMaxLength(30).IsRequired();
                entity.Property(g => g.Description).HasMaxLength(1000);
                entity.Property(g => g.Color).HasMaxLength(7); // For hex color codes
                entity.Property(g => g.GenreOriginCountry).HasMaxLength(100);
                
                // Set default values
                entity.Property(g => g.IsActive).HasDefaultValue(true);
                entity.Property(g => g.TimeStamp).HasDefaultValueSql("NOW()");
                entity.Property(g => g.Key).HasDefaultValue(-1);
                entity.Property(g => g.Volume).HasDefaultValue(-20);
                entity.Property(g => g.CompasMetric).HasDefaultValue(4);
                entity.Property(g => g.Bpm).HasDefaultValue(120);
                
                // Configure relationships
                entity.HasOne(g => g.ParentGenre)
                    .WithMany()
                    .HasForeignKey(g => g.ParentGenreId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(g => g.Cluster)
                    .WithMany(c => c.Genres)
                    .HasForeignKey(g => g.ClusterId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure GenreRelation 
            modelBuilder.Entity<GenreRelation>(entity =>
            {
                entity.HasKey(gr => new { gr.GenreId, gr.RelatedGenreId });
                
                entity.Property(gr => gr.Influence).HasDefaultValue(5);
                
                entity.HasOne(gr => gr.Genre)
                    .WithMany(g => g.RelatedGenresAsSource)
                    .HasForeignKey(gr => gr.GenreId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(gr => gr.RelatedGenre)
                    .WithMany(g => g.RelatedGenresAsTarget)
                    .HasForeignKey(gr => gr.RelatedGenreId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Artist 
            modelBuilder.Entity<Artist>(entity =>
            {
                entity.Property(c => c.Id).HasMaxLength(128);
                entity.Property(c => c.Name).HasMaxLength(30).IsRequired();
                entity.Property(c => c.Biography).HasMaxLength(300);
                entity.Property(c => c.IsActive).HasDefaultValue(true);
                entity.Property(c => c.TimeStamp).HasDefaultValueSql("NOW()");
            });
        }
    }
}