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
            Console.WriteLine("Starting CreateGenreAsync..."); // Log start

            // Validate required fields
            if (string.IsNullOrEmpty(dto.Name))
                throw new ArgumentException("Name is required.");

            // Validate name uniqueness
            if (await _genreRepo.ExistsByNameAsync(dto.Name, dto.IsSubgenre ? dto.ParentGenreId : null))
                throw new ArgumentException($"A {(dto.IsSubgenre ? "subgenre" : "genre")} with this name already exists{(dto.IsSubgenre ? " under this parent genre" : "")}.");

            // Validate subgenre requirements
            if (dto.IsSubgenre && string.IsNullOrEmpty(dto.ParentGenreId))
                throw new ArgumentException("Parent genre is required for subgenres.");

            // Get parent genre if this is a subgenre
            Genre? parentGenre = null;
            if (dto.IsSubgenre && !string.IsNullOrEmpty(dto.ParentGenreId))
            {
                parentGenre = await _genreRepo.GetByIdAsync(dto.ParentGenreId);
                if (parentGenre == null)
                    throw new ArgumentException("Parent genre not found.");
                if (parentGenre.IsSubgenre)
                    throw new ArgumentException("Cannot create a subgenre of another subgenre.");
            }

            // Get cluster if specified (only for main genres, not subgenres)
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
            var genre = new Genre(dto.IsSubgenre)
            {
                Name = dto.Name,
                Description = dto.Description,
                IsSubgenre = dto.IsSubgenre,
                ParentGenreId = dto.IsSubgenre ? dto.ParentGenreId : null, // Fixed: only set if subgenre
                ClusterId = (!dto.IsSubgenre && !string.IsNullOrEmpty(dto.ClusterId)) ? dto.ClusterId : null, // Fixed: only set for main genres
                Key = dto.Key,
                Bpm = (dto.BpmLower + dto.BpmUpper) / 2,
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

            // Ensure ID is set
            genre.SetId();

            Console.WriteLine($"Creating genre with ID: {genre.Id}");
            Console.WriteLine($"ParentGenreId: {genre.ParentGenreId}");
            Console.WriteLine($"ClusterId: {genre.ClusterId}");

            try
            {
                // For main genres without related genres, just add the genre
                if (dto.RelatedGenres == null || !dto.RelatedGenres.Any())
                {
                    await _genreRepo.AddAsync(genre);
                    Console.WriteLine("Genre added without relations");
                }
                else
                {
                    // Save genre with relationships and MGPC calculations
                    await _genreRepo.AddWithRelationsAsync(genre, dto.RelatedGenres);
                    Console.WriteLine("Genre added with relations");
                }

                // If this is a subgenre, calculate MGPC with parent
                if (dto.IsSubgenre && parentGenre != null)
                {
                    var mgpcCalculator = new MgpcCalculator();
                    var mgpcWithParent = mgpcCalculator.Calculate(genre, parentGenre);

                    // Create the parent-child relationship with MGPC
                    await _genreRepo.AddGenreRelationAsync(genre.Id, parentGenre.Id, 10, mgpcWithParent);

                    Console.WriteLine($"Calculated MGPC between subgenre {genre.Name} and parent {parentGenre.Name}: {mgpcWithParent}");
                }

                Console.WriteLine("Genre creation completed successfully."); // Log success
                return genre;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateGenreAsync: {ex}"); // Log the exception
                throw;
            }
        }

        public async Task<Genre?> GetGenreByIdAsync(string id)
        {
            return await _genreRepo.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Genre>> GetAllGenresAsync()
        {
            return await _genreRepo.GetAllAsync();
        }

        public float CalculateMGPC(Genre genreA, Genre genreB)
        {
            var calculator = new MgpcCalculator();
            return calculator.Calculate(genreA, genreB);
        }
    }
}