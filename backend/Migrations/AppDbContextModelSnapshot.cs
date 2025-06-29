﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MusicTree.Repositories;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MusicTree.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("MusicTree.Models.Entities.Album", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("ArtistId")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("CoverImageUrl")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("NOW()");

                    b.Property<int>("DurationSeconds")
                        .HasColumnType("integer");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(true);

                    b.Property<DateTime>("ReleaseDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.HasKey("Id");

                    b.HasIndex("ArtistId");

                    b.HasIndex("IsActive");

                    b.HasIndex("ReleaseDate");

                    b.ToTable("Albums");
                });

            modelBuilder.Entity("MusicTree.Models.Entities.Artist", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("ActivityYears")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<string>("Biography")
                        .HasMaxLength(2000)
                        .HasColumnType("character varying(2000)");

                    b.Property<string>("CoverImageUrl")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(true);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("OriginCountry")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<DateTime>("TimeStamp")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("NOW()");

                    b.HasKey("Id");

                    b.HasIndex("IsActive");

                    b.HasIndex("Name");

                    b.HasIndex("OriginCountry");

                    b.ToTable("Artists");
                });

            modelBuilder.Entity("MusicTree.Models.Entities.ArtistGenre", b =>
                {
                    b.Property<string>("ArtistId")
                        .HasColumnType("character varying(128)");

                    b.Property<string>("GenreId")
                        .HasColumnType("character varying(128)");

                    b.Property<DateTime>("AssociatedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("NOW()");

                    b.Property<float>("InfluenceCoefficient")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("real")
                        .HasDefaultValue(1f);

                    b.HasKey("ArtistId", "GenreId");

                    b.HasIndex("GenreId");

                    b.ToTable("ArtistGenre", t =>
                        {
                            t.HasCheckConstraint("CK_ArtistGenre_NotSubgenre", "NOT EXISTS (SELECT 1 FROM \"Genres\" WHERE \"Id\" = \"GenreId\" AND \"IsSubgenre\" = true)");
                        });
                });

            modelBuilder.Entity("MusicTree.Models.Entities.ArtistMember", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("ActivityPeriod")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<string>("ArtistId")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Instrument")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(true);

                    b.Property<DateTime>("JoinedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("NOW()");

                    b.HasKey("Id");

                    b.HasIndex("ArtistId");

                    b.HasIndex("IsActive");

                    b.ToTable("ArtistMembers");
                });

            modelBuilder.Entity("MusicTree.Models.Entities.ArtistSubgenre", b =>
                {
                    b.Property<string>("ArtistId")
                        .HasColumnType("character varying(128)");

                    b.Property<string>("GenreId")
                        .HasColumnType("character varying(128)");

                    b.Property<DateTime>("AssociatedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("NOW()");

                    b.Property<float>("InfluenceCoefficient")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("real")
                        .HasDefaultValue(1f);

                    b.HasKey("ArtistId", "GenreId");

                    b.HasIndex("GenreId");

                    b.ToTable("ArtistSubgenre", t =>
                        {
                            t.HasCheckConstraint("CK_ArtistSubgenre_IsSubgenre", "EXISTS (SELECT 1 FROM \"Genres\" WHERE \"Id\" = \"GenreId\" AND \"IsSubgenre\" = true)");
                        });
                });

            modelBuilder.Entity("MusicTree.Models.Entities.Cluster", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("Description")
                        .HasMaxLength(300)
                        .HasColumnType("character varying(300)");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(true);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<DateTime>("TimeStamp")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("NOW()");

                    b.HasKey("Id");

                    b.ToTable("Clusters");
                });

            modelBuilder.Entity("MusicTree.Models.Entities.Comment", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("ArtistId")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("AuthorName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("NOW()");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(true);

                    b.HasKey("Id");

                    b.HasIndex("ArtistId");

                    b.HasIndex("CreatedAt");

                    b.HasIndex("IsActive");

                    b.ToTable("Comments");
                });

            modelBuilder.Entity("MusicTree.Models.Entities.Event", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("ArtistId")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Country")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("NOW()");

                    b.Property<string>("Description")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<DateTime>("EventDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(true);

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<string>("Venue")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.HasKey("Id");

                    b.HasIndex("ArtistId");

                    b.HasIndex("EventDate");

                    b.HasIndex("IsActive");

                    b.ToTable("Events");
                });

            modelBuilder.Entity("MusicTree.Models.Entities.Genre", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<int>("AvrgDuration")
                        .HasColumnType("integer");

                    b.Property<int>("Bpm")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(120);

                    b.Property<int>("BpmLower")
                        .HasColumnType("integer");

                    b.Property<int>("BpmUpper")
                        .HasColumnType("integer");

                    b.Property<string>("ClusterId")
                        .HasColumnType("character varying(128)");

                    b.Property<int?>("ColorB")
                        .HasColumnType("integer")
                        .HasColumnName("ColorB");

                    b.Property<int?>("ColorG")
                        .HasColumnType("integer")
                        .HasColumnName("ColorG");

                    b.Property<int?>("ColorR")
                        .HasColumnType("integer")
                        .HasColumnName("ColorR");

                    b.Property<int>("CompasMetric")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(4);

                    b.Property<string>("Description")
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<int?>("GenreCreationYear")
                        .HasColumnType("integer");

                    b.Property<string>("GenreOriginCountry")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<float>("GenreTipicalMode")
                        .HasColumnType("real");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(true);

                    b.Property<bool>("IsSubgenre")
                        .HasColumnType("boolean");

                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(-1);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("ParentGenreId")
                        .HasColumnType("character varying(128)");

                    b.Property<DateTime>("TimeStamp")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("NOW()");

                    b.Property<int>("Volume")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(-20);

                    b.HasKey("Id");

                    b.HasIndex("ClusterId");

                    b.HasIndex("ParentGenreId");

                    b.ToTable("Genres", t =>
                        {
                            t.HasCheckConstraint("CK_Genres_ColorB", "[ColorB] IS NULL OR ([ColorB] >= 0 AND [ColorB] <= 255)");

                            t.HasCheckConstraint("CK_Genres_ColorG", "[ColorG] IS NULL OR ([ColorG] >= 0 AND [ColorG] <= 255)");

                            t.HasCheckConstraint("CK_Genres_ColorR", "[ColorR] IS NULL OR ([ColorR] >= 0 AND [ColorR] <= 255)");

                            t.HasCheckConstraint("CK_Genres_RGB_AllOrNone", "([ColorR] IS NULL AND [ColorG] IS NULL AND [ColorB] IS NULL) OR ([ColorR] IS NOT NULL AND [ColorG] IS NOT NULL AND [ColorB] IS NOT NULL)");
                        });
                });

            modelBuilder.Entity("MusicTree.Models.Entities.GenreRelation", b =>
                {
                    b.Property<string>("GenreId")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("RelatedGenreId")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<int>("Influence")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(5);

                    b.Property<float>("MGPC")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("real")
                        .HasDefaultValue(0f);

                    b.HasKey("GenreId", "RelatedGenreId");

                    b.HasIndex("RelatedGenreId");

                    b.ToTable("GenreRelations", t =>
                        {
                            t.HasCheckConstraint("CK_GenreRelations_Influence", "[Influence] >= 1 AND [Influence] <= 10");

                            t.HasCheckConstraint("CK_GenreRelations_MGPC", "[MGPC] >= 0.0 AND [MGPC] <= 1.0");

                            t.HasCheckConstraint("CK_GenreRelations_NoSelfReference", "[GenreId] != [RelatedGenreId]");
                        });
                });

            modelBuilder.Entity("MusicTree.Models.Entities.Photo", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("ArtistId")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("Caption")
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("NOW()");

                    b.Property<string>("ImageUrl")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(true);

                    b.HasKey("Id");

                    b.HasIndex("ArtistId");

                    b.HasIndex("CreatedAt");

                    b.HasIndex("IsActive");

                    b.ToTable("Photos");
                });

            modelBuilder.Entity("MusicTree.Models.Entities.Album", b =>
                {
                    b.HasOne("MusicTree.Models.Entities.Artist", "Artist")
                        .WithMany("Albums")
                        .HasForeignKey("ArtistId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Artist");
                });

            modelBuilder.Entity("MusicTree.Models.Entities.ArtistGenre", b =>
                {
                    b.HasOne("MusicTree.Models.Entities.Artist", "Artist")
                        .WithMany("ArtistGenres")
                        .HasForeignKey("ArtistId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MusicTree.Models.Entities.Genre", "Genre")
                        .WithMany()
                        .HasForeignKey("GenreId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Artist");

                    b.Navigation("Genre");
                });

            modelBuilder.Entity("MusicTree.Models.Entities.ArtistMember", b =>
                {
                    b.HasOne("MusicTree.Models.Entities.Artist", "Artist")
                        .WithMany("Members")
                        .HasForeignKey("ArtistId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Artist");
                });

            modelBuilder.Entity("MusicTree.Models.Entities.ArtistSubgenre", b =>
                {
                    b.HasOne("MusicTree.Models.Entities.Artist", "Artist")
                        .WithMany("ArtistSubgenres")
                        .HasForeignKey("ArtistId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MusicTree.Models.Entities.Genre", "Genre")
                        .WithMany()
                        .HasForeignKey("GenreId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Artist");

                    b.Navigation("Genre");
                });

            modelBuilder.Entity("MusicTree.Models.Entities.Comment", b =>
                {
                    b.HasOne("MusicTree.Models.Entities.Artist", "Artist")
                        .WithMany("Comments")
                        .HasForeignKey("ArtistId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Artist");
                });

            modelBuilder.Entity("MusicTree.Models.Entities.Event", b =>
                {
                    b.HasOne("MusicTree.Models.Entities.Artist", "Artist")
                        .WithMany("Events")
                        .HasForeignKey("ArtistId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Artist");
                });

            modelBuilder.Entity("MusicTree.Models.Entities.Genre", b =>
                {
                    b.HasOne("MusicTree.Models.Entities.Cluster", "Cluster")
                        .WithMany("Genres")
                        .HasForeignKey("ClusterId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("MusicTree.Models.Entities.Genre", "ParentGenre")
                        .WithMany()
                        .HasForeignKey("ParentGenreId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Cluster");

                    b.Navigation("ParentGenre");
                });

            modelBuilder.Entity("MusicTree.Models.Entities.GenreRelation", b =>
                {
                    b.HasOne("MusicTree.Models.Entities.Genre", "Genre")
                        .WithMany("RelatedGenresAsSource")
                        .HasForeignKey("GenreId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("MusicTree.Models.Entities.Genre", "RelatedGenre")
                        .WithMany("RelatedGenresAsTarget")
                        .HasForeignKey("RelatedGenreId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Genre");

                    b.Navigation("RelatedGenre");
                });

            modelBuilder.Entity("MusicTree.Models.Entities.Photo", b =>
                {
                    b.HasOne("MusicTree.Models.Entities.Artist", "Artist")
                        .WithMany("PhotoGallery")
                        .HasForeignKey("ArtistId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Artist");
                });

            modelBuilder.Entity("MusicTree.Models.Entities.Artist", b =>
                {
                    b.Navigation("Albums");

                    b.Navigation("ArtistGenres");

                    b.Navigation("ArtistSubgenres");

                    b.Navigation("Comments");

                    b.Navigation("Events");

                    b.Navigation("Members");

                    b.Navigation("PhotoGallery");
                });

            modelBuilder.Entity("MusicTree.Models.Entities.Cluster", b =>
                {
                    b.Navigation("Genres");
                });

            modelBuilder.Entity("MusicTree.Models.Entities.Genre", b =>
                {
                    b.Navigation("RelatedGenresAsSource");

                    b.Navigation("RelatedGenresAsTarget");
                });
#pragma warning restore 612, 618
        }
    }
}
