using MusicTree.Models.DTOs;
using MusicTree.Models.Entities;
using MusicTree.Models;
using MusicTree.Repositories;
using MusicTree.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace MusicTree.Services
{
    public class ArtistService : IArtistService
    {
        private readonly ArtistRepository _artistRepo;
        private readonly GenreRepository _genreRepo;
        private readonly ILogger<ArtistService> _logger;

        public ArtistService(
            ArtistRepository artistRepo, 
            GenreRepository genreRepo,
            ILogger<ArtistService> logger)
        {
            _artistRepo = artistRepo;
            _genreRepo = genreRepo;
            _logger = logger;
        }

        public async Task<Artist> CreateArtistAsync(ArtistCreateDto dto)
        {
            _logger.LogInformation("Creating new artist: {ArtistName}", dto.Name);

            try
            {
                // Validate business rules
                await ValidateArtistCreationAsync(dto);

                // Create artist entity
                var artist = new Artist
                {
                    Name = dto.Name,
                    Biography = dto.Biography,
                    OriginCountry = dto.OriginCountry,
                    IsActive = true,
                    TimeStamp = DateTime.UtcNow
                };

                // Save to database
                var createdArtist = await _artistRepo.AddAsync(artist);

                _logger.LogInformation("Artist created successfully: {ArtistId} - {ArtistName}", 
                    createdArtist.Id, createdArtist.Name);

                return createdArtist;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating artist: {ArtistName}", dto.Name);
                throw;
            }
        }

        public async Task<Artist?> GetArtistByIdAsync(string id)
        {
            _logger.LogDebug("Retrieving artist by ID: {ArtistId}", id);

            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    throw new ArgumentException("Artist ID cannot be null or empty", nameof(id));
                }

                var artist = await _artistRepo.GetByIdAsync(id);
                
                if (artist == null)
                {
                    _logger.LogWarning("Artist not found: {ArtistId}", id);
                }

                return artist;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving artist: {ArtistId}", id);
                throw;
            }
        }

        public async Task<PagedResult<Artist>> GetAllArtistsAsync(ArtistSearchParams searchParams)
        {
            _logger.LogDebug("Retrieving artists with search parameters: {@SearchParams}", searchParams);

            try
            {
                // Validate search parameters
                ValidateSearchParams(searchParams);

                // Build query expression
                var filterExpression = BuildFilterExpression(searchParams);

                // Get total count
                var totalCount = await _artistRepo.CountAsync(filterExpression);

                if (totalCount == 0)
                {
                    _logger.LogDebug("No artists found matching search criteria");
                    return new PagedResult<Artist>
                    {
                        Items = Enumerable.Empty<Artist>(),
                        TotalCount = 0,
                        PageNumber = searchParams.PageNumber,
                        PageSize = searchParams.PageSize
                    };
                }

                // Get paginated results
                var artists = await _artistRepo.GetPagedAsync(
                    filterExpression,
                    GetSortExpression(searchParams.SortBy),
                    searchParams.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase),
                    searchParams.PageNumber,
                    searchParams.PageSize
                );

                _logger.LogDebug("Retrieved {Count} artists out of {Total}", 
                    artists.Items.Count(), totalCount);

                return artists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving artists with search parameters");
                throw;
            }
        }

        public async Task<Artist?> UpdateArtistAsync(string id, ArtistCreateDto dto)
        {
            _logger.LogInformation("Updating artist: {ArtistId}", id);

            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    throw new ArgumentException("Artist ID cannot be null or empty", nameof(id));
                }

                var existingArtist = await _artistRepo.GetByIdAsync(id);
                if (existingArtist == null)
                {
                    _logger.LogWarning("Artist not found for update: {ArtistId}", id);
                    return null;
                }

                // Validate business rules for update
                await ValidateArtistUpdateAsync(dto, id);

                // Update properties
                existingArtist.Name = dto.Name;
                existingArtist.Biography = dto.Biography;
                existingArtist.OriginCountry = dto.OriginCountry;

                var updatedArtist = await _artistRepo.UpdateAsync(existingArtist);

                _logger.LogInformation("Artist updated successfully: {ArtistId} - {ArtistName}", 
                    updatedArtist.Id, updatedArtist.Name);

                return updatedArtist;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating artist: {ArtistId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteArtistAsync(string id)
        {
            _logger.LogInformation("Soft deleting artist: {ArtistId}", id);

            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    throw new ArgumentException("Artist ID cannot be null or empty", nameof(id));
                }

                var artist = await _artistRepo.GetByIdAsync(id);
                if (artist == null)
                {
                    _logger.LogWarning("Artist not found for deletion: {ArtistId}", id);
                    return false;
                }

                artist.IsActive = false;
                await _artistRepo.UpdateAsync(artist);

                _logger.LogInformation("Artist soft deleted successfully: {ArtistId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting artist: {ArtistId}", id);
                throw;
            }
        }

        public async Task<bool> ReactivateArtistAsync(string id)
        {
            _logger.LogInformation("Reactivating artist: {ArtistId}", id);

            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    throw new ArgumentException("Artist ID cannot be null or empty", nameof(id));
                }

                var artist = await _artistRepo.GetByIdAsync(id);
                if (artist == null)
                {
                    _logger.LogWarning("Artist not found for reactivation: {ArtistId}", id);
                    return false;
                }

                artist.IsActive = true;
                await _artistRepo.UpdateAsync(artist);

                _logger.LogInformation("Artist reactivated successfully: {ArtistId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating artist: {ArtistId}", id);
                throw;
            }
        }

        // Legacy method for backward compatibility
        public async Task<IEnumerable<Artist>> GetAllArtistAsync(bool includeInactive = false)
        {
            return await _artistRepo.GetAllAsync(includeInactive);
        }

        // Genre relationship methods (placeholder for Sprint 2 User Story 2)
        public async Task<bool> AssociateWithGenreAsync(string artistId, string genreId, float influenceCoefficient = 1.0f)
        {
            _logger.LogInformation("Associating artist {ArtistId} with genre {GenreId}", artistId, genreId);

            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(artistId))
                    throw new ArgumentException("Artist ID cannot be null or empty", nameof(artistId));
                
                if (string.IsNullOrWhiteSpace(genreId))
                    throw new ArgumentException("Genre ID cannot be null or empty", nameof(genreId));

                // Verify artist and genre exist
                var artist = await _artistRepo.GetByIdAsync(artistId);
                if (artist == null)
                    throw new ArgumentException($"Artist with ID '{artistId}' not found");

                var genre = await _genreRepo.GetByIdAsync(genreId);
                if (genre == null)
                    throw new ArgumentException($"Genre with ID '{genreId}' not found");

                // Check if association already exists
                var existingAssociation = await _artistRepo.GetArtistGenreAssociationAsync(artistId, genreId);
                if (existingAssociation != null)
                {
                    _logger.LogWarning("Artist-Genre association already exists: {ArtistId} - {GenreId}", artistId, genreId);
                    return false;
                }

                // Create association
                await _artistRepo.AddArtistGenreAssociationAsync(artistId, genreId, influenceCoefficient);

                _logger.LogInformation("Artist-Genre association created: {ArtistId} - {GenreId}", artistId, genreId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error associating artist with genre: {ArtistId} - {GenreId}", artistId, genreId);
                throw;
            }
        }

        public async Task<bool> RemoveGenreAssociationAsync(string artistId, string genreId)
        {
            _logger.LogInformation("Removing association between artist {ArtistId} and genre {GenreId}", artistId, genreId);

            try
            {
                if (string.IsNullOrWhiteSpace(artistId))
                    throw new ArgumentException("Artist ID cannot be null or empty", nameof(artistId));
                
                if (string.IsNullOrWhiteSpace(genreId))
                    throw new ArgumentException("Genre ID cannot be null or empty", nameof(genreId));

                var success = await _artistRepo.RemoveArtistGenreAssociationAsync(artistId, genreId);
                
                if (success)
                {
                    _logger.LogInformation("Artist-Genre association removed: {ArtistId} - {GenreId}", artistId, genreId);
                }
                else
                {
                    _logger.LogWarning("Artist-Genre association not found: {ArtistId} - {GenreId}", artistId, genreId);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing artist-genre association: {ArtistId} - {GenreId}", artistId, genreId);
                throw;
            }
        }

        public async Task<IEnumerable<Genre>> GetArtistGenresAsync(string artistId)
        {
            _logger.LogDebug("Retrieving genres for artist: {ArtistId}", artistId);

            try
            {
                if (string.IsNullOrWhiteSpace(artistId))
                    throw new ArgumentException("Artist ID cannot be null or empty", nameof(artistId));

                return await _artistRepo.GetArtistGenresAsync(artistId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving genres for artist: {ArtistId}", artistId);
                throw;
            }
        }

        public async Task<IEnumerable<Artist>> GetArtistsByGenreAsync(string genreId)
        {
            _logger.LogDebug("Retrieving artists for genre: {GenreId}", genreId);

            try
            {
                if (string.IsNullOrWhiteSpace(genreId))
                    throw new ArgumentException("Genre ID cannot be null or empty", nameof(genreId));

                return await _artistRepo.GetArtistsByGenreAsync(genreId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving artists for genre: {GenreId}", genreId);
                throw;
            }
        }

        public async Task<double> CalculateArtistDiversityScoreAsync(string artistId)
        {
            _logger.LogDebug("Calculating diversity score for artist: {ArtistId}", artistId);

            try
            {
                if (string.IsNullOrWhiteSpace(artistId))
                    throw new ArgumentException("Artist ID cannot be null or empty", nameof(artistId));

                var genres = await GetArtistGenresAsync(artistId);
                var genreList = genres.ToList();

                if (!genreList.Any())
                    return 0.0;

                // Simple diversity calculation: number of different clusters
                var clusterCount = genreList
                    .Where(g => !string.IsNullOrEmpty(g.ClusterId))
                    .Select(g => g.ClusterId)
                    .Distinct()
                    .Count();

                // Normalize by total number of clusters (assuming 5 clusters max)
                const int maxClusters = 5;
                var diversityScore = (double)clusterCount / maxClusters;

                _logger.LogDebug("Calculated diversity score for artist {ArtistId}: {Score}", artistId, diversityScore);
                return diversityScore;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating diversity score for artist: {ArtistId}", artistId);
                throw;
            }
        }

        public async Task<Dictionary<string, int>> GetArtistCountByCountryAsync()
        {
            _logger.LogDebug("Retrieving artist count by country");

            try
            {
                return await _artistRepo.GetArtistCountByCountryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving artist count by country");
                throw;
            }
        }

        #region Private Helper Methods

        private async Task ValidateArtistCreationAsync(ArtistCreateDto dto)
        {
            // Check name uniqueness
            if (await _artistRepo.ExistsByNameAsync(dto.Name))
            {
                throw new ArgumentException($"An artist with the name '{dto.Name}' already exists");
            }

            // Validate related genres exist (if provided)
            if (dto.ArtistRelatedGenres?.Any() == true)
            {
                foreach (var genreRelation in dto.ArtistRelatedGenres)
                {
                    var genre = await _genreRepo.GetByIdAsync(genreRelation.GenreId);
                    if (genre == null)
                    {
                        throw new ArgumentException($"Genre with ID '{genreRelation.GenreId}' not found");
                    }
                }
            }

            // Validate related subgenres exist (if provided)
            if (dto.ArtistRelatedSubgenres?.Any() == true)
            {
                foreach (var subgenreRelation in dto.ArtistRelatedSubgenres)
                {
                    var subgenre = await _genreRepo.GetByIdAsync(subgenreRelation.GenreId);
                    if (subgenre == null || !subgenre.IsSubgenre)
                    {
                        throw new ArgumentException($"Subgenre with ID '{subgenreRelation.GenreId}' not found");
                    }
                }
            }
        }

        private async Task ValidateArtistUpdateAsync(ArtistCreateDto dto, string currentId)
        {
            // Check name uniqueness (excluding current artist)
            var existingArtist = await _artistRepo.GetByNameAsync(dto.Name);
            if (existingArtist != null && existingArtist.Id != currentId)
            {
                throw new ArgumentException($"An artist with the name '{dto.Name}' already exists");
            }
        }

        private static void ValidateSearchParams(ArtistSearchParams searchParams)
        {
            if (searchParams.PageNumber < 1)
                throw new ArgumentException("Page number must be greater than 0");

            if (searchParams.PageSize < 1 || searchParams.PageSize > 100)
                throw new ArgumentException("Page size must be between 1 and 100");
        }

        private static Expression<Func<Artist, bool>> BuildFilterExpression(ArtistSearchParams searchParams)
        {
            Expression<Func<Artist, bool>> expression = a => true;

            if (!searchParams.IncludeInactive)
            {
                expression = expression.And(a => a.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(searchParams.Name))
            {
                var searchTerm = searchParams.Name.ToLower();
                expression = expression.And(a => a.Name.ToLower().Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(searchParams.OriginCountry))
            {
                var country = searchParams.OriginCountry.ToLower();
                expression = expression.And(a => a.OriginCountry.ToLower().Contains(country));
            }

            return expression;
        }

        private static Expression<Func<Artist, object>> GetSortExpression(string sortBy)
        {
            return sortBy.ToLower() switch
            {
                "name" => a => a.Name,
                "origincountry" => a => a.OriginCountry,
                "createdat" => a => a.TimeStamp,
                _ => a => a.Name
            };
        }

        #endregion
    }

    // Extension method for combining expressions
    public static class ExpressionExtensions
    {
        public static Expression<Func<T, bool>> And<T>(
            this Expression<Func<T, bool>> first,
            Expression<Func<T, bool>> second)
        {
            var parameter = first.Parameters[0];
            var leftVisitor = new ReplaceExpressionVisitor(second.Parameters[0], parameter);
            var left = leftVisitor.Visit(second.Body);
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(first.Body, left), parameter);
        }

        private class ReplaceExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression _oldValue;
            private readonly Expression _newValue;

            public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
            {
                _oldValue = oldValue;
                _newValue = newValue;
            }

            public override Expression Visit(Expression node)
            {
                return node == _oldValue ? _newValue : base.Visit(node);
            }
        }
    }
}