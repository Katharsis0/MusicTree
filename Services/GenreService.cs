using MusicTree.Models.DTOs;
using MusicTree.Models.Entities;
using MusicTree.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MusicTree.Services.Interfaces;
using MusicTree.Utils;

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

            // Validate name uniqueness (considering subgenre context)
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

            // Get cluster if specified (only for non-subgenres)
            Cluster? cluster = null;
            if (!string.IsNullOrEmpty(dto.ClusterId) && !dto.IsSubgenre)
            {
                cluster = await _clusterRepo.GetByIdAsync(dto.ClusterId);
                if (cluster == null)
                    throw new ArgumentException("Cluster not found.");
            }

            // Validate BPM range
            if (dto.BpmLower > dto.BpmUpper)
                throw new ArgumentException("BPM lower bound cannot be greater than upper bound.");

            // Create the genre entity
            var genre = new Genre
            {
                Name = dto.Name,
                Description = dto.Description,
                IsSubgenre = dto.IsSubgenre,
                ParentGenreId = parentGenre?.Id,
                ClusterId = cluster?.Id,
                Key = dto.Key,
                BpmLower = dto.BpmLower,
                BpmUpper = dto.BpmUpper,
                Color = dto.IsSubgenre ? null : dto.Color,
                GenreCreationYear = dto.GenreCreationYear,
                GenreOriginCountry = dto.GenreOriginCountry,
                GenreTipicalMode = dto.GenreTipicalMode,
                Volume = dto.Volume,
                CompasMetric = dto.CompasMetric,
                AvrgDuration = dto.AvrgDuration
            };

            // Save genre with relationships and MGPC calculations
            await _genreRepo.AddWithRelationsAsync(genre, dto.RelatedGenres);

            // If this is a subgenre, calculate MGPC with parent
            if (dto.IsSubgenre && parentGenre != null)
            {
                var mgpcCalculator = new MgpcCalculator();
                var mgpcWithParent = mgpcCalculator.Calculate(genre, parentGenre);
                // This MGPC can be stored in a separate parent-child relationship table if needed
            }

            return genre;
        }

        public float CalculateMGPC(Genre genreA, Genre genreB)
        {
            var calculator = new MgpcCalculator();
            return calculator.Calculate(genreA, genreB);
        }
    }
}