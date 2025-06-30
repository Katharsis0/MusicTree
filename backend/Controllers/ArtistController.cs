using Microsoft.AspNetCore.Mvc;
using MusicTree.Models.DTOs;
using MusicTree.Models.Responses;
using MusicTree.Services.Interfaces;

namespace MusicTree.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArtistsController : ControllerBase
    {
        private readonly IArtistService _artistService;

        public ArtistsController(IArtistService artistService)
        {
            _artistService = artistService;
        }

        /// <summary>
        /// Create a new artist
        /// </summary>
        /// <param name="dto">Artist creation data</param>
        /// <returns>Created artist with generated ID</returns>
        [HttpPost]
        public async Task<IActionResult> CreateArtist([FromForm] ArtistCreateDto dto)
        {
            try
            {
                // Validate model state
                if (!ModelState.IsValid)
                {
                    return BadRequest(new 
                    { 
                        error = "All required information must be provided. Please check your input and try again.",
                        details = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var artist = await _artistService.CreateArtistAsync(dto);
                
                return CreatedAtAction(
                    nameof(GetArtistById), 
                    new { id = artist.Id }, 
                    new 
                    { 
                        id = artist.Id,
                        name = artist.Name,
                        originCountry = artist.OriginCountry,
                        activityYears = artist.ActivityYears,
                        createdAt = artist.TimeStamp,
                        genreCount = artist.GenreCount,
                        albumCount = artist.AlbumCount,
                        memberCount = artist.ActiveMemberCount,
                        message = "Artist created successfully"
                    });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in CreateArtist: {ex}");
                return StatusCode(500, new { 
                    error = "Error occurred while processing the request. Please try again later."
                });
            }
        }

        /// <summary>
        /// Get a specific artist by ID
        /// </summary>
        /// <param name="id">Artist ID</param>
        /// <returns>Artist details</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetArtistById(string id)
        {
            try
            {
                var artist = await _artistService.GetArtistByIdAsync(id);
                
                if (artist == null)
                {
                    return NotFound(new { error = $"Artist with ID '{id}' not found" });
                }

                var response = new ArtistDetailResponseDto
                {
                    Id = artist.Id,
                    Name = artist.Name,
                    Biography = artist.Biography,
                    OriginCountry = artist.OriginCountry,
                    ActivityYears = artist.ActivityYears,
                    CoverImageUrl = artist.CoverImageUrl,
                    IsActive = artist.IsActive,
                    CreatedAt = artist.TimeStamp,
                    AssociatedGenres = artist.ArtistGenres.Select(ag => new GenreBasicDto
                    {
                        Id = ag.Genre.Id,
                        Name = ag.Genre.Name,
                        IsSubgenre = ag.Genre.IsSubgenre,
                        ParentGenreName = ag.Genre.ParentGenre?.Name,
                        InfluenceCoefficient = ag.InfluenceCoefficient
                    }).ToList(),
                    AssociatedSubgenres = artist.ArtistSubgenres.Select(asg => new GenreBasicDto
                    {
                        Id = asg.Genre.Id,
                        Name = asg.Genre.Name,
                        IsSubgenre = asg.Genre.IsSubgenre,
                        ParentGenreName = asg.Genre.ParentGenre?.Name,
                        InfluenceCoefficient = asg.InfluenceCoefficient
                    }).ToList(),
                    Members = artist.Members.Select(m => new ArtistMemberResponseDto
                    {
                        Id = m.Id,
                        FullName = m.FullName,
                        Instrument = m.Instrument,
                        ActivityPeriod = m.ActivityPeriod,
                        IsActive = m.IsActive
                    }).ToList(),
                    Albums = artist.Albums.Select(a => new AlbumResponseDto
                    {
                        Id = a.Id,
                        Title = a.Title,
                        ReleaseDate = a.ReleaseDate,
                        CoverImageUrl = a.CoverImageUrl,
                        FormattedDuration = a.FormattedDuration,
                        DurationSeconds = a.DurationSeconds
                    }).ToList(),
                    Statistics = new ArtistStatisticsDto
                    {
                        GenreCount = artist.ArtistGenres.Count,
                        SubgenreCount = artist.ArtistSubgenres.Count,
                        AlbumCount = artist.AlbumCount,
                        ActiveMemberCount = artist.ActiveMemberCount,
                        CommentCount = artist.Comments.Count(c => c.IsActive),
                        PhotoCount = artist.PhotoGallery.Count(p => p.IsActive),
                        EventCount = artist.Events.Count(e => e.IsActive && e.EventDate >= DateTime.UtcNow)
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetArtistById: {ex}");
                return StatusCode(500, new { 
                    error = "A system error occurred. Please try again later." 
                });
            }
        }

        /// <summary>
        /// Get all artists with optional filtering and pagination
        /// </summary>
        /// <param name="searchParams">Search and pagination parameters</param>
        /// <returns>Paginated list of artists</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllArtists([FromQuery] ArtistSearchParams searchParams)
        {
            try
            {
                var result = await _artistService.GetAllArtistsAsync(searchParams);

                var response = new PagedResponse<ArtistListItemDto>
                {
                    Items = result.Items.Select(a => new ArtistListItemDto
                    {
                        Id = a.Id,
                        Name = a.Name,
                        OriginCountry = a.OriginCountry,
                        ActivityYears = a.ActivityYears,
                        IsActive = a.IsActive,
                        CreatedAt = a.TimeStamp,
                        GenreCount = a.GenreCount,
                        AlbumCount = a.AlbumCount,
                        MemberCount = a.ActiveMemberCount
                    }).ToList(),
                    TotalCount = result.TotalCount,
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize,
                    TotalPages = result.TotalPages
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllArtists: {ex}");
                return StatusCode(500, new { 
                    error = "A system error occurred. Please try again later." 
                });
            }
        }

        /// <summary>
        /// Update an existing artist
        /// </summary>
        /// <param name="id">Artist ID</param>
        /// <param name="dto">Updated artist data</param>
        /// <returns>Updated artist</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateArtist(string id, [FromForm] ArtistCreateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new 
                    { 
                        error = "All required information must be provided. Please check your input and try again.",
                        details = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var artist = await _artistService.UpdateArtistAsync(id, dto);
                
                if (artist == null)
                {
                    return NotFound(new { error = $"Artist with ID '{id}' not found" });
                }

                return Ok(new 
                { 
                    id = artist.Id,
                    name = artist.Name,
                    originCountry = artist.OriginCountry,
                    activityYears = artist.ActivityYears,
                    updatedAt = DateTime.UtcNow,
                    message = "Artist updated successfully"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in UpdateArtist: {ex}");
                return StatusCode(500, new { 
                    error = "Error occurred while processing the request. Please try again later."
                });
            }
        }

        /// <summary>
        /// Soft delete an artist (set as inactive)
        /// </summary>
        /// <param name="id">Artist ID</param>
        /// <returns>Success confirmation</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArtist(string id)
        {
            try
            {
                var success = await _artistService.DeleteArtistAsync(id);
                
                if (!success)
                {
                    return NotFound(new { error = $"Artist with ID '{id}' not found" });
                }

                return Ok(new { message = "Artist deactivated successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteArtist: {ex}");
                return StatusCode(500, new { 
                    error = "Error occurred while processing the request. Please try again later."
                });
            }
        }

        /// <summary>
        /// Reactivate a previously deactivated artist
        /// </summary>
        /// <param name="id">Artist ID</param>
        /// <returns>Success confirmation</returns>
        [HttpPatch("{id}/reactivate")]
        public async Task<IActionResult> ReactivateArtist(string id)
        {
            try
            {
                var success = await _artistService.ReactivateArtistAsync(id);
                
                if (!success)
                {
                    return NotFound(new { error = $"Artist with ID '{id}' not found" });
                }

                return Ok(new { message = "Artist reactivated successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ReactivateArtist: {ex}");
                return StatusCode(500, new { 
                    error = "Error occurred while processing the request. Please try again later."
                });
            }
        }

        /// <summary>
        /// Get artist statistics by country
        /// </summary>
        /// <returns>Dictionary of country statistics</returns>
        [HttpGet("statistics/by-country")]
        public async Task<IActionResult> GetArtistStatsByCountry()
        {
            try
            {
                var stats = await _artistService.GetArtistCountByCountryAsync();
                return Ok(new { statistics = stats, generatedAt = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetArtistStatsByCountry: {ex}");
                return StatusCode(500, new { 
                    error = "Error occurred while processing the request. Please try again later."
                });
            }
        }
        
        [HttpGet("by-genres")]
        public async Task<IActionResult> GetArtistsByMultipleGenres(
        [FromQuery] string genreIds,
        [FromQuery] string logic = "OR",
        [FromQuery] bool includeInactive = false)
        {
        try
        {
            if (string.IsNullOrWhiteSpace(genreIds))
            {
                return BadRequest(new { error = "At least one genre ID is required" });
            }

            var genreIdList = genreIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                     .Select(id => id.Trim())
                                     .ToList();

            if (!genreIdList.Any())
            {
                return BadRequest(new { error = "Invalid genre IDs format" });
            }

            var useAndLogic = logic.Equals("AND", StringComparison.OrdinalIgnoreCase);
            var artists = await _artistService.GetArtistsByMultipleGenresAsync(genreIdList, useAndLogic, includeInactive);

            var response = artists.Select(a => new
            {
                id = a.Id,
                name = a.Name,
                originCountry = a.OriginCountry,
                activityYears = a.ActivityYears,
                isActive = a.IsActive,
                genreCount = a.GenreCount,
                albumCount = a.AlbumCount,
                memberCount = a.ActiveMemberCount,
                genres = a.ArtistGenres.Select(ag => new { id = ag.GenreId, name = ag.Genre.Name }),
                subgenres = a.ArtistSubgenres.Select(asg => new { id = asg.GenreId, name = asg.Genre.Name })
            }).ToList();

            return Ok(new
            {
                logic = logic.ToUpper(),
                genreIds = genreIdList,
                count = response.Count,
                artists = response
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetArtistsByMultipleGenres: {ex}");
            return StatusCode(500, new { error = "Error occurred while processing the request" });
        }
        }

        /// <summary>
        /// Get artists by cluster
        /// </summary>
        /// <param name="clusterId">Cluster ID</param>
        /// <param name="includeInactive">Include inactive artists</param>
        /// <returns>Artists associated with the cluster through their genres</returns>
        [HttpGet("by-cluster/{clusterId}")]
        public async Task<IActionResult> GetArtistsByCluster(string clusterId, [FromQuery] bool includeInactive = false)
        {
        try
        {
            if (string.IsNullOrWhiteSpace(clusterId))
            {
                return BadRequest(new { error = "Cluster ID is required" });
            }

            var artists = await _artistService.GetArtistsByClusterAsync(clusterId, includeInactive);
            var artistList = artists.ToList();

            var response = artistList.Select(a => new
            {
                id = a.Id,
                name = a.Name,
                originCountry = a.OriginCountry,
                activityYears = a.ActivityYears,
                isActive = a.IsActive,
                genreCount = a.GenreCount,
                albumCount = a.AlbumCount,
                memberCount = a.ActiveMemberCount,
                clusterGenres = a.ArtistGenres
                    .Where(ag => ag.Genre.ClusterId == clusterId)
                    .Select(ag => new { id = ag.GenreId, name = ag.Genre.Name })
            }).ToList();

            return Ok(new
            {
                clusterId = clusterId,
                count = response.Count,
                artists = response
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetArtistsByCluster: {ex}");
            return StatusCode(500, new { error = "Error occurred while processing the request" });
        }
        }

        /// <summary>
        /// Get artists with advanced statistics filtering
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetArtistsWithStatistics(
        [FromQuery] int? minGenreCount = null,
        [FromQuery] int? maxGenreCount = null,
        [FromQuery] int? minAlbumCount = null,
        [FromQuery] int? maxAlbumCount = null,
        [FromQuery] int? minMemberCount = null,
        [FromQuery] int? maxMemberCount = null,
        [FromQuery] bool includeInactive = false)
        {
        try
        {
            var artists = await _artistService.GetArtistsWithStatisticsAsync(
                minGenreCount, maxGenreCount,
                minAlbumCount, maxAlbumCount,
                minMemberCount, maxMemberCount,
                includeInactive);

            var artistList = artists.ToList();

            var response = artistList.Select(a => new
            {
                id = a.Id,
                name = a.Name,
                originCountry = a.OriginCountry,
                activityYears = a.ActivityYears,
                isActive = a.IsActive,
                statistics = new
                {
                    genreCount = a.GenreCount,
                    albumCount = a.AlbumCount,
                    memberCount = a.ActiveMemberCount,
                    commentCount = a.Comments.Count(c => c.IsActive),
                    photoCount = a.PhotoGallery.Count(p => p.IsActive),
                    eventCount = a.Events.Count(e => e.IsActive && e.EventDate >= DateTime.UtcNow)
                }
            }).ToList();

            return Ok(new
            {
                filters = new
                {
                    minGenreCount,
                    maxGenreCount,
                    minAlbumCount,
                    maxAlbumCount,
                    minMemberCount,
                    maxMemberCount,
                    includeInactive
                },
                count = response.Count,
                artists = response
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetArtistsWithStatistics: {ex}");
            return StatusCode(500, new { error = "Error occurred while processing the request" });
        }
        }

        /// <summary>
        /// Search artists with full-text search capabilities
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchArtists(
        [FromQuery] string searchTerm,
        [FromQuery] string searchFields = "name,biography,originCountry",
        [FromQuery] bool exactMatch = false,
        [FromQuery] bool caseSensitive = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
        {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest(new { error = "Search term is required" });
            }

            var searchParams = new ArtistSearchParams
            {
                Name = searchFields.Contains("name") ? searchTerm : null,
                OriginCountry = searchFields.Contains("originCountry") ? searchTerm : null,
                ExactNameMatch = exactMatch,
                CaseSensitive = caseSensitive,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            // If biography search is needed, you'll need to extend the filtering logic
            var result = await _artistService.GetAllArtistsAsync(searchParams);

            var response = new
            {
                searchTerm,
                searchFields = searchFields.Split(','),
                options = new { exactMatch, caseSensitive },
                pagination = new
                {
                    pageNumber = result.PageNumber,
                    pageSize = result.PageSize,
                    totalPages = result.TotalPages,
                    totalCount = result.TotalCount
                },
                artists = result.Items.Select(a => new
                {
                    id = a.Id,
                    name = a.Name,
                    biography = a.Biography,
                    originCountry = a.OriginCountry,
                    activityYears = a.ActivityYears,
                    isActive = a.IsActive,
                    genreCount = a.GenreCount,
                    albumCount = a.AlbumCount,
                    memberCount = a.ActiveMemberCount
                }).ToList()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SearchArtists: {ex}");
            return StatusCode(500, new { error = "Error occurred while processing the request" });
        }
        }

        /// <summary>
        /// Get artists by activity status
        /// </summary>
        [HttpGet("by-activity-status")]
        public async Task<IActionResult> GetArtistsByActivityStatus(
        [FromQuery] bool currentlyActive,
        [FromQuery] int? activeFromYear = null,
        [FromQuery] int? activeToYear = null)
        {
        try
        {
            var searchParams = new ArtistSearchParams
            {
                CurrentlyActive = currentlyActive,
                ActiveFromYear = activeFromYear,
                ActiveToYear = activeToYear,
                PageSize = 100 // Get more results for this type of query
            };

            var result = await _artistService.GetAllArtistsAsync(searchParams);

            var response = result.Items.Select(a => new
            {
                id = a.Id,
                name = a.Name,
                originCountry = a.OriginCountry,
                activityYears = a.ActivityYears,
                isCurrentlyActive = a.ActivityYears.ToLower().Contains("presente") || 
                                   a.ActivityYears.ToLower().Contains("present"),
                genreCount = a.GenreCount,
                albumCount = a.AlbumCount
            }).ToList();

            return Ok(new
            {
                filters = new { currentlyActive, activeFromYear, activeToYear },
                count = response.Count,
                totalCount = result.TotalCount,
                artists = response
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetArtistsByActivityStatus: {ex}");
            return StatusCode(500, new { error = "Error occurred while processing the request" });
        }
        }
    }
}