using MusicTree.Models.DTOs;
using MusicTree.Models.Entities;
using MusicTree.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MusicTree.Services.Interfaces;

namespace MusicTree.Services
{
    public class GenreService : IGenreService
    {
        private readonly GenreRepository _genreRepo;
        private readonly ClusterRepository _clusterRepo;

        public GenreService(GenreRepository genreRepo, ClusterRepository clusterRepo)
        {
            _genreRepo = genreRepo;
            _clusterRepo = clusterRepo;
        }

        public async Task<Genre> CreateGenreAsync(GenreCreateDto dto)
        {
            // Validate required fields
            if (string.IsNullOrEmpty(dto.Name))
                throw new ArgumentException("Name is required.");

            // Validate name uniqueness
            if (await _genreRepo.ExistsByNameAsync(dto.Name, dto.IsSubgenre ? dto.ParentGenreId : null))
                throw new ArgumentException("Genre with this name already exists.");

            // Validate subgenre requirements
            if (dto.IsSubgenre && string.IsNullOrEmpty(dto.ParentGenreId))
                throw new ArgumentException("Parent genre is required for subgenres.");

            // Get parent genre if this is a subgenre
            Genre? parentGenre = null;
            if (dto.IsSubgenre)
            {
                parentGenre = await _genreRepo.GetByIdAsync(dto.ParentGenreId);
                if (parentGenre == null)
                    throw new ArgumentException("Parent genre not found.");
                if (parentGenre.IsSubgenre)
                    throw new ArgumentException("Cannot create a subgenre of another subgenre.");
            }

            // Get cluster if specified
            Cluster? cluster = null;
            if (!string.IsNullOrEmpty(dto.ClusterId) && !dto.IsSubgenre)
            {
                cluster = await _clusterRepo.GetByIdAsync(dto.ClusterId);
                if (cluster == null)
                    throw new ArgumentException("Cluster not found.");
            }

            // Create the genre
            var genre = new Genre
            {
                Id = GenerateGenreId(dto.IsSubgenre),
                Name = dto.Name,
                Description = dto.Description,
                IsSubgenre = dto.IsSubgenre,
                ParentGenre = parentGenre,
                Cluster = cluster,
                Color = dto.IsSubgenre ? null : dto.Color,
                GenreCreationYear = dto.GenreCreationYear,
                GenreOriginCountry = dto.GenreOriginCountry,
                GenreTipicalMode = dto.GenreTipicalMode,
                Bpm = dto.Bpm,
                Volume = dto.Volume,
                CompasMetric = dto.CompasMetric,
                AvrgDuration = dto.AvrgDuration,
                RelatedGenres = new List<Genre>()
            };

            // Handle related genres and calculate MGPC
            if (dto.RelatedGenres != null && dto.RelatedGenres.Any())
            {
                foreach (var relation in dto.RelatedGenres)
                {
                    var relatedGenre = await _genreRepo.GetByIdAsync(relation.GenreId);
                    if (relatedGenre != null)
                    {
                        genre.RelatedGenres.Add(relatedGenre);
                        // Here you would store the MGPC calculation
                    }
                }
            }

            await _genreRepo.AddAsync(genre);
            return genre;
        }

        private string GenerateGenreId(bool isSubgenre)
        {
            var randomId = GenerateRandomId(12);
            return isSubgenre ? $"G-{randomId}S-{GenerateRandomId(12)}" : $"G-{randomId}S-000000000000";
        }

        private string GenerateRandomId(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // Implement MGPC calculation based on the formula in the spec
        public float CalculateMGPC(Genre genreA, Genre genreB)
        {
            //TODO
            throw new NotImplementedException();
        }
    }
}