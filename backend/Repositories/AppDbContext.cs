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
        public DbSet<ArtistMember> ArtistMembers { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<FanUser> Fanaticos { get; set; }
        public DbSet<FanUserGenero> FanUserGeneros { get; set; }

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
                entity.Property(g => g.GenreOriginCountry).HasMaxLength(100);

                // RGB Color configuration
                entity.Property(g => g.ColorR).HasColumnName("ColorR");
                entity.Property(g => g.ColorG).HasColumnName("ColorG");
                entity.Property(g => g.ColorB).HasColumnName("ColorB");

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

                // Add check constraints for RGB values (if supported by provider)
                entity.HasCheckConstraint("CK_Genres_ColorR", "[ColorR] IS NULL OR ([ColorR] >= 0 AND [ColorR] <= 255)");
                entity.HasCheckConstraint("CK_Genres_ColorG", "[ColorG] IS NULL OR ([ColorG] >= 0 AND [ColorG] <= 255)");
                entity.HasCheckConstraint("CK_Genres_ColorB", "[ColorB] IS NULL OR ([ColorB] >= 0 AND [ColorB] <= 255)");

                // Ensure all RGB components are provided together or none at all
                entity.HasCheckConstraint("CK_Genres_RGB_AllOrNone",
                    "([ColorR] IS NULL AND [ColorG] IS NULL AND [ColorB] IS NULL) OR ([ColorR] IS NOT NULL AND [ColorG] IS NOT NULL AND [ColorB] IS NOT NULL)");

                // Exclude computed properties from database mapping
                entity.Ignore(g => g.RgbColor);
                entity.Ignore(g => g.HexColor);
                entity.Ignore(g => g.RelatedGenres);
            });

            // Configure GenreRelation 
            modelBuilder.Entity<GenreRelation>(entity =>
            {
                entity.HasKey(gr => new { gr.GenreId, gr.RelatedGenreId });

                entity.Property(gr => gr.GenreId).HasMaxLength(128);
                entity.Property(gr => gr.RelatedGenreId).HasMaxLength(128);
                entity.Property(gr => gr.Influence).HasDefaultValue(5);
                entity.Property(gr => gr.MGPC).HasDefaultValue(0.0f);

                entity.HasOne(gr => gr.Genre)
                    .WithMany(g => g.RelatedGenresAsSource)
                    .HasForeignKey(gr => gr.GenreId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(gr => gr.RelatedGenre)
                    .WithMany(g => g.RelatedGenresAsTarget)
                    .HasForeignKey(gr => gr.RelatedGenreId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Add check constraints
                entity.HasCheckConstraint("CK_GenreRelations_Influence", "[Influence] >= 1 AND [Influence] <= 10");
                entity.HasCheckConstraint("CK_GenreRelations_MGPC", "[MGPC] >= 0.0 AND [MGPC] <= 1.0");
                entity.HasCheckConstraint("CK_GenreRelations_NoSelfReference", "[GenreId] != [RelatedGenreId]");
            });

            // Configure Artist 
            modelBuilder.Entity<Artist>(entity =>
            {
                entity.Property(a => a.Id).HasMaxLength(128);
                entity.Property(a => a.Name).HasMaxLength(100).IsRequired();
                entity.Property(a => a.Biography).HasMaxLength(2000);
                entity.Property(a => a.OriginCountry).HasMaxLength(100).IsRequired();
                entity.Property(a => a.IsActive).HasDefaultValue(true);
                entity.Property(a => a.TimeStamp).HasDefaultValueSql("NOW()");

                // Index for performance
                entity.HasIndex(a => a.Name);
                entity.HasIndex(a => a.OriginCountry);
                entity.HasIndex(a => a.IsActive);
            });

            // Configure ArtistGenre junction table
            modelBuilder.Entity<ArtistGenre>(entity =>
            {
                entity.HasKey(ag => new { ag.ArtistId, ag.GenreId });

                entity.Property(ag => ag.InfluenceCoefficient).HasDefaultValue(1.0f);
                entity.Property(ag => ag.AssociatedDate).HasDefaultValueSql("NOW()");

                entity.HasOne(ag => ag.Artist)
                    .WithMany(a => a.ArtistGenres)
                    .HasForeignKey(ag => ag.ArtistId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ag => ag.Genre)
                    .WithMany()
                    .HasForeignKey(ag => ag.GenreId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Ensure genre is not a subgenre
                entity.HasCheckConstraint("CK_ArtistGenre_NotSubgenre",
                    "NOT EXISTS (SELECT 1 FROM \"Genres\" WHERE \"Id\" = \"GenreId\" AND \"IsSubgenre\" = true)");
            });

            // Configure ArtistSubgenre junction table
            modelBuilder.Entity<ArtistSubgenre>(entity =>
            {
                entity.HasKey(asq => new { asq.ArtistId, asq.GenreId });

                entity.Property(asq => asq.InfluenceCoefficient).HasDefaultValue(1.0f);
                entity.Property(asq => asq.AssociatedDate).HasDefaultValueSql("NOW()");

                entity.HasOne(asq => asq.Artist)
                    .WithMany(a => a.ArtistSubgenres)
                    .HasForeignKey(asq => asq.ArtistId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(asq => asq.Genre)
                    .WithMany()
                    .HasForeignKey(asq => asq.GenreId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Ensure genre is a subgenre
                entity.HasCheckConstraint("CK_ArtistSubgenre_IsSubgenre",
                    "EXISTS (SELECT 1 FROM \"Genres\" WHERE \"Id\" = \"GenreId\" AND \"IsSubgenre\" = true)");
            });

            // FIXED: Configure ArtistMember with correct column name
            modelBuilder.Entity<ArtistMember>(entity =>
            {
                entity.Property(am => am.Id).HasMaxLength(128).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(am => am.ArtistId).HasMaxLength(128);
                entity.Property(am => am.FullName).HasMaxLength(200).IsRequired();
                entity.Property(am => am.Instrument).HasMaxLength(100);
                entity.Property(am => am.ActivityPeriod).HasMaxLength(100);
                entity.Property(am => am.IsActive).HasDefaultValue(true);
                entity.Property(am => am.CreatedAt).HasDefaultValueSql("NOW()");

                entity.HasOne(am => am.Artist)
                    .WithMany(a => a.Members)
                    .HasForeignKey(am => am.ArtistId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Album
            modelBuilder.Entity<Album>(entity =>
            {
                entity.Property(alb => alb.Id).HasMaxLength(128).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(alb => alb.ArtistId).HasMaxLength(128);
                entity.Property(alb => alb.Title).HasMaxLength(200).IsRequired();
                entity.Property(alb => alb.IsActive).HasDefaultValue(true);
                entity.Property(alb => alb.CreatedAt).HasDefaultValueSql("NOW()");

                entity.HasOne(alb => alb.Artist)
                    .WithMany(a => a.Albums)
                    .HasForeignKey(alb => alb.ArtistId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(alb => alb.ArtistId);
            });

            // Configure Comment
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.Property(c => c.Id).HasMaxLength(128).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(c => c.ArtistId).HasMaxLength(128);
                entity.Property(c => c.Content).IsRequired();
                entity.Property(c => c.AuthorName).HasMaxLength(100).IsRequired();
                entity.Property(c => c.IsActive).HasDefaultValue(true);
                entity.Property(c => c.CreatedAt).HasDefaultValueSql("NOW()");

                entity.HasOne(c => c.Artist)
                    .WithMany(a => a.Comments)
                    .HasForeignKey(c => c.ArtistId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(c => c.ArtistId);
            });

            // Configure Photo
            modelBuilder.Entity<Photo>(entity =>
            {
                entity.Property(p => p.Id).HasMaxLength(128).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(p => p.ArtistId).HasMaxLength(128);
                entity.Property(p => p.ImageUrl).IsRequired();
                entity.Property(p => p.Caption).HasMaxLength(500);
                entity.Property(p => p.IsActive).HasDefaultValue(true);
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("NOW()");

                entity.HasOne(p => p.Artist)
                    .WithMany(a => a.PhotoGallery)
                    .HasForeignKey(p => p.ArtistId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(p => p.ArtistId);
            });

            // Configure Event
            modelBuilder.Entity<Event>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(128).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.ArtistId).HasMaxLength(128);
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Venue).HasMaxLength(200);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

                entity.HasOne(e => e.Artist)
                    .WithMany(a => a.Events)
                    .HasForeignKey(e => e.ArtistId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.ArtistId);
                entity.HasIndex(e => e.EventDate);
            });

            // Configure FanUser
            modelBuilder.Entity<FanUser>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.Property(f => f.Id).HasMaxLength(128).HasDefaultValueSql("gen_random_uuid()");
                
                entity.Property(f => f.Nickname).HasMaxLength(50).IsRequired();
                entity.Property(f => f.Password).HasMaxLength(200).IsRequired();
                entity.Property(f => f.Nombre).HasMaxLength(100).IsRequired();
                entity.Property(f => f.Pais).HasMaxLength(100).IsRequired();
                entity.Property(f => f.Avatar).HasMaxLength(300);
                
                entity.Property(f => f.FechaCreacion).HasDefaultValueSql("NOW()");
            });

            // Configure FanUserGenero (many-to-many between FanUser and Genre)
            modelBuilder.Entity<FanUserGenero>(entity =>
            {
                entity.HasKey(fg => new { fg.FanUserId, fg.GeneroId });

                entity.HasOne(fg => fg.FanUser)
                    .WithMany(f => f.GenerosFavoritos)
                    .HasForeignKey(fg => fg.FanUserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(fg => fg.Genero)
                    .WithMany()
                    .HasForeignKey(fg => fg.GeneroId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


        }

        // Override SaveChanges to add additional validation
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Validate RGB colors before saving
            var genreEntries = ChangeTracker.Entries<Genre>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in genreEntries)
            {
                var genre = entry.Entity;
                
                // Validate RGB color consistency
                var hasR = genre.ColorR.HasValue;
                var hasG = genre.ColorG.HasValue;
                var hasB = genre.ColorB.HasValue;

                if (hasR != hasG || hasG != hasB)
                {
                    throw new InvalidOperationException(
                        $"Genre '{genre.Name}': All RGB color components (R, G, B) must be provided together or none at all.");
                }

                // Validate RGB ranges
                if (hasR && (genre.ColorR < 0 || genre.ColorR > 255))
                {
                    throw new InvalidOperationException(
                        $"Genre '{genre.Name}': ColorR must be between 0 and 255.");
                }

                if (hasG && (genre.ColorG < 0 || genre.ColorG > 255))
                {
                    throw new InvalidOperationException(
                        $"Genre '{genre.Name}': ColorG must be between 0 and 255.");
                }

                if (hasB && (genre.ColorB < 0 || genre.ColorB > 255))
                {
                    throw new InvalidOperationException(
                        $"Genre '{genre.Name}': ColorB must be between 0 and 255.");
                }

                // Validate that subgenres don't have colors
                if (genre.IsSubgenre && (hasR || hasG || hasB))
                {
                    throw new InvalidOperationException(
                        $"Subgenre '{genre.Name}': Subgenres cannot have colors assigned.");
                }
            }

            // Validate GenreRelations
            var relationEntries = ChangeTracker.Entries<GenreRelation>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in relationEntries)
            {
                var relation = entry.Entity;

                // Validate influence range
                if (relation.Influence < 1 || relation.Influence > 10)
                {
                    throw new InvalidOperationException(
                        $"Genre relation influence must be between 1 and 10. Got: {relation.Influence}");
                }

                // Validate MGPC range
                if (relation.MGPC < 0.0f || relation.MGPC > 1.0f)
                {
                    throw new InvalidOperationException(
                        $"Genre relation MGPC must be between 0.0 and 1.0. Got: {relation.MGPC}");
                }

                // Prevent self-references
                if (relation.GenreId == relation.RelatedGenreId)
                {
                    throw new InvalidOperationException(
                        "A genre cannot have a relationship with itself.");
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}