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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ArtistService(
            ArtistRepository artistRepo, 
            GenreRepository genreRepo,
            ILogger<ArtistService> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _artistRepo = artistRepo;
            _genreRepo = genreRepo;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
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
                    ActivityYears = dto.ActivityYears,
                    IsActive = true,
                    TimeStamp = DateTime.UtcNow
                };

                // Handle cover image upload
                if (dto.CoverImage != null)
                {
                    artist.CoverImageUrl = await SaveImageAsync(dto.CoverImage, "artists", artist.Id);
                }

                // Save artist first to get ID
                var createdArtist = await _artistRepo.AddAsync(artist);

                // Add genre associations
                if (dto.ArtistRelatedGenres?.Any() == true)
                {
                    foreach (var genreRelation in dto.ArtistRelatedGenres)
                    {
                        await _artistRepo.AddArtistGenreAssociationAsync(
                            createdArtist.Id, 
                            genreRelation.GenreId, 
                            genreRelation.InfluenceCoefficient);
                    }
                }

                // Add subgenre associations
                if (dto.ArtistRelatedSubgenres?.Any() == true)
                {
                    foreach (var subgenreRelation in dto.ArtistRelatedSubgenres)
                    {
                        await _artistRepo.AddArtistSubgenreAssociationAsync(
                            createdArtist.Id, 
                            subgenreRelation.GenreId, 
                            subgenreRelation.InfluenceCoefficient);
                    }
                }

                // Add members
                if (dto.Members?.Any() == true)
                {
                    foreach (var memberDto in dto.Members)
                    {
                        await _artistRepo.AddArtistMemberAsync(createdArtist.Id, memberDto);
                    }
                }

                // Add albums
                if (dto.Albums?.Any() == true)
                {
                    foreach (var albumDto in dto.Albums)
                    {
                        var albumId = $"{createdArtist.Id}-D-{GenerateRandomId(12)}";
                        
                        string? albumCoverUrl = null;
                        if (albumDto.CoverImage != null)
                        {
                            albumCoverUrl = await SaveImageAsync(albumDto.CoverImage, "albums", albumId);
                        }

                        await _artistRepo.AddAlbumAsync(createdArtist.Id, albumDto, albumCoverUrl);
                    }
                }

                // Initialize empty collections (auto-generated as per requirements)
                await InitializeArtistCollectionsAsync(createdArtist.Id);

                _logger.LogInformation("Artist created successfully: {ArtistId} - {ArtistName}", 
                    createdArtist.Id, createdArtist.Name);

                // Return complete artist with all relationships
                return await _artistRepo.GetByIdAsync(createdArtist.Id) ?? createdArtist;
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

                // Get paginated results
                var artists = await _artistRepo.GetPagedAsync(
                    filterExpression,
                    GetSortExpression(searchParams.SortBy),
                    searchParams.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase),
                    searchParams.PageNumber,
                    searchParams.PageSize
                );

                _logger.LogDebug("Retrieved {Count} artists out of {Total}", 
                    artists.Items.Count(), artists.TotalCount);

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

                // Update basic properties
                existingArtist.Name = dto.Name;
                existingArtist.Biography = dto.Biography;
                existingArtist.OriginCountry = dto.OriginCountry;
                existingArtist.ActivityYears = dto.ActivityYears;

                // Handle cover image update
                if (dto.CoverImage != null)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(existingArtist.CoverImageUrl))
                    {
                        await DeleteImageAsync(existingArtist.CoverImageUrl);
                    }
                    
                    existingArtist.CoverImageUrl = await SaveImageAsync(dto.CoverImage, "artists", existingArtist.Id);
                }

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

            // Validate related genres exist
            if (dto.ArtistRelatedGenres?.Any() == true)
            {
                foreach (var genreRelation in dto.ArtistRelatedGenres)
                {
                    var genre = await _genreRepo.GetByIdAsync(genreRelation.GenreId);
                    if (genre == null)
                    {
                        throw new ArgumentException($"Genre with ID '{genreRelation.GenreId}' not found");
                    }
                    if (genre.IsSubgenre)
                    {
                        throw new ArgumentException($"Genre '{genreRelation.GenreId}' is a subgenre. Use ArtistRelatedSubgenres for subgenres.");
                    }
                }
            }

            // Validate related subgenres exist
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

            if (!string.IsNullOrWhiteSpace(searchParams.ActivityYears))
            {
                var activityYears = searchParams.ActivityYears.ToLower();
                expression = expression.And(a => a.ActivityYears.ToLower().Contains(activityYears));
            }

            if (searchParams.HasAlbums.HasValue)
            {
                if (searchParams.HasAlbums.Value)
                {
                    expression = expression.And(a => a.Albums.Any());
                }
                else
                {
                    expression = expression.And(a => !a.Albums.Any());
                }
            }

            if (searchParams.MinAlbumCount.HasValue)
            {
                expression = expression.And(a => a.Albums.Count >= searchParams.MinAlbumCount.Value);
            }

            if (searchParams.MaxAlbumCount.HasValue)
            {
                expression = expression.And(a => a.Albums.Count <= searchParams.MaxAlbumCount.Value);
            }

            return expression;
        }

        private static Expression<Func<Artist, object>> GetSortExpression(string sortBy)
        {
            return sortBy.ToLower() switch
            {
                "name" => a => a.Name,
                "origincountry" => a => a.OriginCountry,
                "activityyears" => a => a.ActivityYears,
                "createdat" => a => a.TimeStamp,
                "albumcount" => a => a.Albums.Count,
                "genrecount" => a => a.ArtistGenres.Count + a.ArtistSubgenres.Count,
                _ => a => a.Name
            };
        }

        private async Task<string> SaveImageAsync(IFormFile image, string folder, string entityId)
        {
            var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", folder);
            Directory.CreateDirectory(uploadsPath);

            var fileName = $"{entityId}_{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
            var filePath = Path.Combine(uploadsPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await image.CopyToAsync(stream);

            return $"/uploads/{folder}/{fileName}";
        }

        private async Task DeleteImageAsync(string imageUrl)
        {
            try
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete image: {ImageUrl}", imageUrl);
            }
        }

        private async Task InitializeArtistCollectionsAsync(string artistId)
        {
            // Initialize empty comment thread
            await _artistRepo.InitializeCommentThreadAsync(artistId);
            
            // Initialize empty photo gallery
            await _artistRepo.InitializePhotoGalleryAsync(artistId);
            
            // Initialize empty events calendar
            await _artistRepo.InitializeEventsCalendarAsync(artistId);
        }

        private static string GenerateRandomId(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        #endregion

        #region Genre Relationship Methods (Missing implementations)

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

                if (influenceCoefficient < 0.1f || influenceCoefficient > 10.0f)
                    throw new ArgumentException("Influence coefficient must be between 0.1 and 10.0", nameof(influenceCoefficient));

                // Verify artist exists
                var artist = await _artistRepo.GetByIdAsync(artistId);
                if (artist == null)
                    throw new ArgumentException($"Artist with ID '{artistId}' not found");

                // Verify genre exists and is not a subgenre
                var genre = await _genreRepo.GetByIdAsync(genreId);
                if (genre == null)
                    throw new ArgumentException($"Genre with ID '{genreId}' not found");

                if (genre.IsSubgenre)
                    throw new ArgumentException($"Cannot associate with subgenre '{genreId}'. Use AssociateWithSubgenreAsync for subgenres.");

                // Check if association already exists
                var existingAssociation = await _artistRepo.GetArtistGenreAssociationAsync(artistId, genreId);
                if (existingAssociation != null)
                {
                    _logger.LogWarning("Artist-Genre association already exists: {ArtistId} - {GenreId}", artistId, genreId);
                    return false;
                }

                // Create association
                await _artistRepo.AddArtistGenreAssociationAsync(artistId, genreId, influenceCoefficient);

                _logger.LogInformation("Artist-Genre association created: {ArtistId} - {GenreId} with influence {Influence}", 
                    artistId, genreId, influenceCoefficient);
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

                // Remove genre association
                var genreRemoved = await _artistRepo.RemoveArtistGenreAssociationAsync(artistId, genreId);
                
                // Try subgenre association if genre association was not found
                var subgenreRemoved = false;
                if (!genreRemoved)
                {
                    subgenreRemoved = await _artistRepo.RemoveArtistSubgenreAssociationAsync(artistId, genreId);
                }

                var success = genreRemoved || subgenreRemoved;
                
                if (success)
                {
                    _logger.LogInformation("Artist-Genre/Subgenre association removed: {ArtistId} - {GenreId}", artistId, genreId);
                }
                else
                {
                    _logger.LogWarning("Artist-Genre/Subgenre association not found: {ArtistId} - {GenreId}", artistId, genreId);
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

                // Get both main genres and subgenres
                var mainGenres = await _artistRepo.GetArtistGenresAsync(artistId);
                var subgenres = await _artistRepo.GetArtistSubgenresAsync(artistId);

                // Combine and return all genres
                var allGenres = mainGenres.Concat(subgenres).ToList();

                _logger.LogDebug("Retrieved {Count} genres for artist {ArtistId}", allGenres.Count, artistId);
                return allGenres;
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

                // Check if it's a main genre or subgenre and get artists accordingly
                var genre = await _genreRepo.GetByIdAsync(genreId);
                if (genre == null)
                    throw new ArgumentException($"Genre with ID '{genreId}' not found");

                IEnumerable<Artist> artists;
                if (genre.IsSubgenre)
                {
                    artists = await _artistRepo.GetArtistsBySubgenreAsync(genreId);
                }
                else
                {
                    artists = await _artistRepo.GetArtistsByGenreAsync(genreId);
                }

                var artistList = artists.ToList();
                _logger.LogDebug("Retrieved {Count} artists for genre {GenreId}", artistList.Count, genreId);
                return artistList;
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

                var artist = await _artistRepo.GetByIdAsync(artistId);
                if (artist == null)
                    throw new ArgumentException($"Artist with ID '{artistId}' not found");

                // Get all genres associated with the artist
                var allGenres = await GetArtistGenresAsync(artistId);
                var genreList = allGenres.ToList();

                if (!genreList.Any())
                {
                    _logger.LogDebug("Artist {ArtistId} has no genres, diversity score: 0.0", artistId);
                    return 0.0;
                }

                // Calculate diversity based on multiple factors
                
                // 1. Cluster diversity (different clusters represented)
                var clusterCount = genreList
                    .Where(g => !string.IsNullOrEmpty(g.ClusterId))
                    .Select(g => g.ClusterId)
                    .Distinct()
                    .Count();

                // 2. Genre count diversity (number of different genres)
                var mainGenreCount = genreList.Count(g => !g.IsSubgenre);
                var subgenreCount = genreList.Count(g => g.IsSubgenre);

                // 3. Musical attribute diversity (BPM range coverage, different modes, etc.)
                var bpmRangeDiversity = CalculateBpmRangeDiversity(genreList);
                var modeDiversity = CalculateModeDiversity(genreList);
                var keyDiversity = CalculateKeyDiversity(genreList);

                // Weighted diversity score calculation
                const double clusterWeight = 0.4;
                const double genreCountWeight = 0.2;
                const double bpmWeight = 0.15;
                const double modeWeight = 0.15;
                const double keyWeight = 0.1;

                // Normalize cluster diversity (assuming max 5 clusters exist)
                const int maxClusters = 5;
                var normalizedClusterDiversity = Math.Min(1.0, (double)clusterCount / maxClusters);

                // Normalize genre count diversity (logarithmic scale to prevent huge numbers from dominating)
                var normalizedGenreCountDiversity = Math.Min(1.0, Math.Log(mainGenreCount + subgenreCount + 1) / Math.Log(10));

                var diversityScore = 
                    (clusterWeight * normalizedClusterDiversity) +
                    (genreCountWeight * normalizedGenreCountDiversity) +
                    (bpmWeight * bpmRangeDiversity) +
                    (modeWeight * modeDiversity) +
                    (keyWeight * keyDiversity);

                // Ensure score is between 0 and 1
                diversityScore = Math.Max(0.0, Math.Min(1.0, diversityScore));

                _logger.LogDebug("Calculated diversity score for artist {ArtistId}: {Score} " +
                                "(Clusters: {Clusters}, Genres: {MainGenres}+{Subgenres}, BPM: {BPM:F2}, Mode: {Mode:F2}, Key: {Key:F2})", 
                                artistId, diversityScore, clusterCount, mainGenreCount, subgenreCount, 
                                bpmRangeDiversity, modeDiversity, keyDiversity);

                return diversityScore;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating diversity score for artist: {ArtistId}", artistId);
                throw;
            }
        }

        #endregion

        #region Additional Helper Methods for Diversity Calculation

        private static double CalculateBpmRangeDiversity(List<Genre> genres)
        {
            if (!genres.Any()) return 0.0;

            var minBpm = genres.Min(g => g.BpmLower);
            var maxBpm = genres.Max(g => g.BpmUpper);
            var bpmRange = maxBpm - minBpm;

            // Normalize BPM range diversity (assuming max range of 200 BPM is very diverse)
            return Math.Min(1.0, bpmRange / 200.0);
        }

        private static double CalculateModeDiversity(List<Genre> genres)
        {
            if (!genres.Any()) return 0.0;

            var uniqueModes = genres
                .Select(g => g.GenreTipicalMode)
                .Distinct()
                .Count();

            // More unique modes = higher diversity (max of 1.0 for having both major and minor)
            return Math.Min(1.0, (uniqueModes - 1) / 1.0);
        }

        private static double CalculateKeyDiversity(List<Genre> genres)
        {
            if (!genres.Any()) return 0.0;

            var definedKeys = genres
                .Where(g => g.Key >= 0 && g.Key <= 11)
                .Select(g => g.Key)
                .Distinct()
                .Count();

            // Normalize key diversity (12 different keys maximum)
            return Math.Min(1.0, definedKeys / 12.0);
        }

        #endregion

        #region Additional Association Methods (Optional but useful)

        public async Task<bool> AssociateWithSubgenreAsync(string artistId, string subgenreId, float influenceCoefficient = 1.0f)
        {
            _logger.LogInformation("Associating artist {ArtistId} with subgenre {SubgenreId}", artistId, subgenreId);

            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(artistId))
                    throw new ArgumentException("Artist ID cannot be null or empty", nameof(artistId));
                
                if (string.IsNullOrWhiteSpace(subgenreId))
                    throw new ArgumentException("Subgenre ID cannot be null or empty", nameof(subgenreId));

                if (influenceCoefficient < 0.1f || influenceCoefficient > 10.0f)
                    throw new ArgumentException("Influence coefficient must be between 0.1 and 10.0", nameof(influenceCoefficient));

                // Verify artist exists
                var artist = await _artistRepo.GetByIdAsync(artistId);
                if (artist == null)
                    throw new ArgumentException($"Artist with ID '{artistId}' not found");

                // Verify subgenre exists and is actually a subgenre
                var subgenre = await _genreRepo.GetByIdAsync(subgenreId);
                if (subgenre == null || !subgenre.IsSubgenre)
                    throw new ArgumentException($"Subgenre with ID '{subgenreId}' not found");

                // Check if association already exists
                var existingAssociation = await _artistRepo.GetArtistSubgenreAssociationAsync(artistId, subgenreId);
                if (existingAssociation != null)
                {
                    _logger.LogWarning("Artist-Subgenre association already exists: {ArtistId} - {SubgenreId}", artistId, subgenreId);
                    return false;
                }

                // Create association
                await _artistRepo.AddArtistSubgenreAssociationAsync(artistId, subgenreId, influenceCoefficient);

                _logger.LogInformation("Artist-Subgenre association created: {ArtistId} - {SubgenreId} with influence {Influence}", 
                    artistId, subgenreId, influenceCoefficient);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error associating artist with subgenre: {ArtistId} - {SubgenreId}", artistId, subgenreId);
                throw;
            }
        }

        public async Task<string?> GetMostInfluentialGenreAsync(string artistId)
        {
            _logger.LogDebug("Finding most influential genre for artist: {ArtistId}", artistId);

            try
            {
                if (string.IsNullOrWhiteSpace(artistId))
                    throw new ArgumentException("Artist ID cannot be null or empty", nameof(artistId));

                var artist = await _artistRepo.GetByIdAsync(artistId);
                if (artist == null)
                    return null;

                // Find genre with highest influence coefficient
                var mostInfluentialGenre = artist.ArtistGenres
                    .Concat(artist.ArtistSubgenres.Select(asg => new ArtistGenre 
                    { 
                        Genre = asg.Genre, 
                        InfluenceCoefficient = asg.InfluenceCoefficient 
                    }))
                    .OrderByDescending(ag => ag.InfluenceCoefficient)
                    .FirstOrDefault();

                var result = mostInfluentialGenre?.Genre.Name;
                _logger.LogDebug("Most influential genre for artist {ArtistId}: {GenreName}", artistId, result ?? "None");
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding most influential genre for artist: {ArtistId}", artistId);
                throw;
            }
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