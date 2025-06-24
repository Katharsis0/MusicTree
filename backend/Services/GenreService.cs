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

            // Validate related genres exist before creating
            if (dto.RelatedGenres != null && dto.RelatedGenres.Any())
            {
                foreach (var relation in dto.RelatedGenres)
                {
                    var relatedGenre = await _genreRepo.GetByIdAsync(relation.GenreId);
                    if (relatedGenre == null)
                        throw new ArgumentException($"Related genre with ID '{relation.GenreId}' not found.");
                }
            }

            // Create the genre entity
            var genre = new Genre(dto.IsSubgenre)
            {
                Name = dto.Name,
                Description = dto.Description,
                IsSubgenre = dto.IsSubgenre,
                ParentGenreId = dto.IsSubgenre ? dto.ParentGenreId : null,
                ClusterId = (!dto.IsSubgenre && !string.IsNullOrEmpty(dto.ClusterId)) ? dto.ClusterId : null,
                Key = dto.Key,
                Bpm = (dto.BpmLower + dto.BpmUpper) / 2,
                BpmLower = dto.BpmLower,
                BpmUpper = dto.BpmUpper,
                GenreCreationYear = dto.GenreCreationYear,
                GenreOriginCountry = dto.GenreOriginCountry,
                GenreTipicalMode = dto.GenreTipicalMode,
                Volume = dto.Volume,
                CompasMetric = dto.CompasMetric,
                AvrgDuration = dto.AvrgDuration
            };

            // Set RGB color if provided (only for main genres)
            if (!dto.IsSubgenre)
            {
                genre.SetRgbColor(dto.ColorR, dto.ColorG, dto.ColorB);
            }

            // Ensure ID is set
            genre.SetId();

            Console.WriteLine($"Creating genre with ID: {genre.Id}");
            Console.WriteLine($"ParentGenreId: {genre.ParentGenreId}");
            Console.WriteLine($"ClusterId: {genre.ClusterId}");
            Console.WriteLine($"RGB Color: R={genre.ColorR}, G={genre.ColorG}, B={genre.ColorB}");

            try
            {
                // First, save the genre
                await _genreRepo.AddAsync(genre);
                Console.WriteLine("Genre saved successfully");

                // Now handle relationships and MGCP calculations
                var mgpcCalculator = new MgpcCalculator();

                // If this is a subgenre, create automatic relationship with parent
                if (dto.IsSubgenre && parentGenre != null)
                {
                    var mgpcWithParent = mgpcCalculator.Calculate(genre, parentGenre);
                    await _genreRepo.AddGenreRelationAsync(genre.Id, parentGenre.Id, 10, mgpcWithParent);
                    Console.WriteLine($"Created parent-child relationship: MGPC={mgpcWithParent}");
                }

                // Handle explicit related genres with influence relationships
                if (dto.RelatedGenres != null && dto.RelatedGenres.Any())
                {
                    Console.WriteLine($"Processing {dto.RelatedGenres.Count} related genres...");
                    
                    foreach (var relation in dto.RelatedGenres)
                    {
                        var relatedGenre = await _genreRepo.GetByIdAsync(relation.GenreId);
                        if (relatedGenre != null)
                        {
                            // Calculate MGPC between the new genre and related genre
                            var mgpc = mgpcCalculator.Calculate(genre, relatedGenre);
                            
                            // Create the relationship with the specified influence strength
                            await _genreRepo.AddGenreRelationAsync(
                                genre.Id, 
                                relatedGenre.Id, 
                                relation.InfluenceStrength, 
                                mgpc);

                            Console.WriteLine($"Created influence relationship with {relatedGenre.Name}: Influence={relation.InfluenceStrength}, MGPC={mgpc}");
                        }
                    }
                }

                Console.WriteLine("Genre creation completed successfully.");
                
                // Reload the genre with all relationships to return complete data
                var completeGenre = await _genreRepo.GetByIdAsync(genre.Id);
                return completeGenre ?? genre;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateGenreAsync: {ex}");
                throw new InvalidOperationException($"Failed to create genre: {ex.Message}", ex);
            }
        }

        public async Task<Genre?> GetGenreByIdAsync(string id)
        {
            try
            {
                return await _genreRepo.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetGenreByIdAsync: {ex}");
                throw new InvalidOperationException($"Failed to retrieve genre: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Genre>> GetAllGenresAsync()
        {
            try
            {
                return await _genreRepo.GetAllAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllGenresAsync: {ex}");
                throw new InvalidOperationException($"Failed to retrieve genres: {ex.Message}", ex);
            }
        }

        public float CalculateMGPC(Genre genreA, Genre genreB)
        {
            try
            {
                if (genreA == null || genreB == null)
                    throw new ArgumentNullException("Both genres must be provided for MGPC calculation");

                var calculator = new MgpcCalculator();
                var result = calculator.Calculate(genreA, genreB);
                
                Console.WriteLine($"MGPC calculated between '{genreA.Name}' and '{genreB.Name}': {result}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating MGPC: {ex}");
                throw new InvalidOperationException($"Failed to calculate MGPC: {ex.Message}", ex);
            }
        }

        // Additional method to get genres with detailed relationship information
        public async Task<Genre?> GetGenreWithRelationshipsAsync(string id)
        {
            try
            {
                var genre = await _genreRepo.GetByIdAsync(id);
                if (genre == null) return null;

                // Log relationship information for debugging
                Console.WriteLine($"Genre '{genre.Name}' has {genre.RelatedGenresAsSource.Count} outgoing relationships");
                Console.WriteLine($"Genre '{genre.Name}' has {genre.RelatedGenresAsTarget.Count} incoming relationships");

                return genre;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetGenreWithRelationshipsAsync: {ex}");
                throw new InvalidOperationException($"Failed to retrieve genre with relationships: {ex.Message}", ex);
            }
        }

        // Method to recalculate MGPC for existing relationships
        public async Task<bool> RecalculateMGPCForGenreAsync(string genreId)
        {
            try
            {
                var genre = await _genreRepo.GetByIdAsync(genreId);
                if (genre == null) return false;

                var calculator = new MgpcCalculator();
                
                // Recalculate for all outgoing relationships
                foreach (var relation in genre.RelatedGenresAsSource)
                {
                    var newMgpc = calculator.Calculate(genre, relation.RelatedGenre);
                    relation.MGPC = newMgpc;
                    Console.WriteLine($"Updated MGPC for {genre.Name} -> {relation.RelatedGenre.Name}: {newMgpc}");
                }

                // Update the database would require additional repository method
                // This is for future implementation
                Console.WriteLine($"MGPC recalculation completed for genre: {genre.Name}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RecalculateMGPCForGenreAsync: {ex}");
                return false;
            }
        }
    }
}