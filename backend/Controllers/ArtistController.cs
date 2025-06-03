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
    }
}