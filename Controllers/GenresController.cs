using Microsoft.AspNetCore.Mvc;
using MusicTree.Models.DTOs;
using MusicTree.Models.Entities;
using MusicTree.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace MusicTree.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GenresController : ControllerBase
    {
        private readonly IGenreService _genreService;

        public GenresController(IGenreService genreService)
        {
            _genreService = genreService;
        }

        /// <summary>
        /// Create a new genre or subgenre
        /// </summary>
        /// <param name="dto">Genre creation data</param>
        /// <returns>Created genre with generated ID</returns>
        [HttpPost]
        public async Task<IActionResult> CreateGenre([FromBody] GenreCreateDto dto)
        {
            try
            {
                // Validate model state first
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { 
                        error = "All required information must be provided. Please check your input and try again.",
                        details = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                // Additional validation using IValidatableObject
                var validationContext = new ValidationContext(dto);
                var validationResults = new List<ValidationResult>();
                if (!Validator.TryValidateObject(dto, validationContext, validationResults, true))
                {
                    return BadRequest(new { 
                        error = "Validation failed. Please check your input and try again.",
                        details = validationResults.Select(r => r.ErrorMessage)
                    });
                }

                var genre = await _genreService.CreateGenreAsync(dto);
                
                return CreatedAtAction(
                    nameof(GetGenreById), 
                    new { id = genre.Id }, 
                    new { 
                        id = genre.Id,
                        name = genre.Name,
                        isSubgenre = genre.IsSubgenre,
                        createdAt = genre.TimeStamp,
                        color = genre.RgbColor,
                        message = $"{(genre.IsSubgenre ? "Subgenre" : "Genre")} created successfully"
                    });
            }
            catch (ArgumentException ex)
            {
                // Handle specific business logic errors
                if (ex.Message.Contains("already exists"))
                {
                    return Conflict(new { error = ex.Message });
                }
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                // System error - log this in production
                Console.WriteLine($"Unexpected error in CreateGenre: {ex}");
                return StatusCode(500, new { 
                    error = "A system error occurred. Please try again later.",
                    details = ex.Message // Remove this in production
                });
            }
        }

        /// <summary>
        /// Get a specific genre by its ID
        /// </summary>
        /// <param name="id">Genre ID</param>
        /// <returns>Genre details</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGenreById(string id)
        {
            try
            {
                var genre = await _genreService.GetGenreByIdAsync(id);
                
                if (genre == null)
                {
                    return NotFound(new { error = $"Genre with ID '{id}' not found" });
                }

                // Return detailed genre information
                var response = new
                {
                    id = genre.Id,
                    name = genre.Name,
                    description = genre.Description,
                    isSubgenre = genre.IsSubgenre,
                    parentGenreId = genre.ParentGenreId,
                    parentGenreName = genre.ParentGenre?.Name,
                    clusterId = genre.ClusterId,
                    clusterName = genre.Cluster?.Name,
                    musicalAttributes = new
                    {
                        key = genre.Key,
                        bpmRange = new { lower = genre.BpmLower, upper = genre.BpmUpper },
                        averageBpm = genre.Bpm,
                        typicalMode = genre.GenreTipicalMode,
                        volume = genre.Volume,
                        compasMetric = genre.CompasMetric,
                        averageDuration = genre.AvrgDuration
                    },
                    metadata = new
                    {
                        color = new { 
                            rgb = genre.RgbColor,
                            hex = genre.HexColor,
                            components = genre.ColorR.HasValue ? new { r = genre.ColorR, g = genre.ColorG, b = genre.ColorB } : null
                        },
                        creationYear = genre.GenreCreationYear,
                        originCountry = genre.GenreOriginCountry,
                        isActive = genre.IsActive,
                        createdAt = genre.TimeStamp
                    },
                    relatedGenres = genre.RelatedGenresAsSource.Select(r => new
                    {
                        id = r.RelatedGenreId,
                        name = r.RelatedGenre.Name,
                        influence = r.Influence,
                        mgpc = r.MGPC
                    }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetGenreById: {ex}");
                return StatusCode(500, new { 
                    error = "A system error occurred. Please try again later." 
                });
            }
        }

        /// <summary>
        /// Get all genres with optional filtering
        /// </summary>
        /// <param name="includeSubgenres">Include subgenres in the list (default: true)</param>
        /// <param name="includeInactive">Include inactive genres (default: false)</param>
        /// <param name="parentGenreId">Filter subgenres by parent genre ID</param>
        /// <param name="compasMetric">Filter by compás metric (time signature)</param>
        /// <param name="minBpm">Minimum BPM filter</param>
        /// <param name="maxBpm">Maximum BPM filter</param>
        /// <returns>List of genres</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllGenres(
            [FromQuery] bool includeSubgenres = true, 
            [FromQuery] bool includeInactive = false,
            [FromQuery] string? parentGenreId = null,
            [FromQuery] int? compasMetric = null,
            [FromQuery] int? minBpm = null,
            [FromQuery] int? maxBpm = null)
        {
            try
            {
                var genres = await _genreService.GetAllGenresAsync();
                
                // Apply filters
                var filteredGenres = genres.AsEnumerable();
                
                if (!includeInactive)
                {
                    filteredGenres = filteredGenres.Where(g => g.IsActive);
                }
                
                if (!includeSubgenres)
                {
                    filteredGenres = filteredGenres.Where(g => !g.IsSubgenre);
                }
                
                if (!string.IsNullOrEmpty(parentGenreId))
                {
                    filteredGenres = filteredGenres.Where(g => g.ParentGenreId == parentGenreId);
                }

                // NEW: Filter by compás metric
                if (compasMetric.HasValue)
                {
                    filteredGenres = filteredGenres.Where(g => g.CompasMetric == compasMetric.Value);
                }

                // NEW: Filter by BPM range
                if (minBpm.HasValue)
                {
                    filteredGenres = filteredGenres.Where(g => g.BpmUpper >= minBpm.Value);
                }

                if (maxBpm.HasValue)
                {
                    filteredGenres = filteredGenres.Where(g => g.BpmLower <= maxBpm.Value);
                }

                // Check if list is empty
                if (!filteredGenres.Any())
                {
                    return Ok(new { 
                        message = "No genres found matching the specified criteria.",
                        appliedFilters = new
                        {
                            includeSubgenres,
                            includeInactive,
                            parentGenreId,
                            compasMetric,
                            minBpm,
                            maxBpm
                        },
                        genres = new List<object>()
                    });
                }

                // Transform to response format
                var response = filteredGenres.Select(g => new
                {
                    id = g.Id,
                    name = g.Name,
                    description = g.Description,
                    isSubgenre = g.IsSubgenre,
                    parentGenreName = g.ParentGenre?.Name,
                    clusterName = g.Cluster?.Name,
                    isActive = g.IsActive,
                    createdAt = g.TimeStamp,
                    musicalAttributes = new
                    {
                        bpmRange = new { lower = g.BpmLower, upper = g.BpmUpper },
                        typicalMode = g.GenreTipicalMode,
                        volume = g.Volume,
                        compasMetric = g.CompasMetric,
                        averageDuration = g.AvrgDuration
                    },
                    color = new
                    {
                        rgb = g.RgbColor,
                        hex = g.HexColor,
                        components = g.ColorR.HasValue ? new { r = g.ColorR, g = g.ColorG, b = g.ColorB } : null
                    }
                })
                .OrderBy(g => g.name) // Per requirements: alphabetical order
                .ToList();

                return Ok(new { 
                    count = response.Count,
                    appliedFilters = new
                    {
                        includeSubgenres,
                        includeInactive,
                        parentGenreId,
                        compasMetric,
                        minBpm,
                        maxBpm
                    },
                    genres = response 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllGenres: {ex}");
                return StatusCode(500, new { 
                    error = "A system error occurred. Please try again later." 
                });
            }
        }

        /// <summary>
        /// Calculate MGPC between two genres
        /// </summary>
        /// <param name="genreIdA">First genre ID</param>
        /// <param name="genreIdB">Second genre ID</param>
        /// <returns>MGPC coefficient</returns>
        [HttpGet("mgpc")]
        public async Task<IActionResult> CalculateMGPC([FromQuery] string genreIdA, [FromQuery] string genreIdB)
        {
            try
            {
                if (string.IsNullOrEmpty(genreIdA) || string.IsNullOrEmpty(genreIdB))
                {
                    return BadRequest(new { error = "Both genre IDs are required" });
                }

                var genreA = await _genreService.GetGenreByIdAsync(genreIdA);
                var genreB = await _genreService.GetGenreByIdAsync(genreIdB);

                if (genreA == null)
                {
                    return NotFound(new { error = $"Genre A with ID '{genreIdA}' not found" });
                }

                if (genreB == null)
                {
                    return NotFound(new { error = $"Genre B with ID '{genreIdB}' not found" });
                }

                var mgpc = _genreService.CalculateMGPC(genreA, genreB);

                return Ok(new
                {
                    genreA = new { id = genreA.Id, name = genreA.Name },
                    genreB = new { id = genreB.Id, name = genreB.Name },
                    mgpc = mgpc,
                    interpretation = mgpc switch
                    {
                        >= 0.8f => "Very High Similarity",
                        >= 0.6f => "High Similarity", 
                        >= 0.4f => "Moderate Similarity",
                        >= 0.2f => "Low Similarity",
                        _ => "Very Low Similarity"
                    },
                    calculatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CalculateMGPC: {ex}");
                return StatusCode(500, new { 
                    error = "A system error occurred. Please try again later." 
                });
            }
        }

        /// <summary>
        /// Get genres filtered by compás metric specifically
        /// </summary>
        /// <param name="compasMetric">Compás metric to filter by</param>
        /// <returns>List of genres with specified compás metric</returns>
        [HttpGet("by-compas/{compasMetric}")]
        public async Task<IActionResult> GetGenresByCompas(int compasMetric)
        {
            try
            {
                if (compasMetric < 0 || compasMetric > 8)
                {
                    return BadRequest(new { error = "Compás metric must be between 0 and 8" });
                }

                var genres = await _genreService.GetAllGenresAsync();
                var filteredGenres = genres.Where(g => g.CompasMetric == compasMetric && g.IsActive).ToList();

                var response = filteredGenres.Select(g => new
                {
                    id = g.Id,
                    name = g.Name,
                    compasMetric = g.CompasMetric,
                    bpmRange = new { lower = g.BpmLower, upper = g.BpmUpper },
                    typicalMode = g.GenreTipicalMode,
                    isSubgenre = g.IsSubgenre,
                    parentGenreName = g.ParentGenre?.Name
                }).ToList();

                return Ok(new
                {
                    compasMetric = compasMetric,
                    compasDescription = compasMetric switch
                    {
                        0 => "Undefined",
                        2 => "2/4 time",
                        3 => "3/4 time (Waltz)",
                        4 => "4/4 time (Common)",
                        6 => "6/8 time",
                        _ => $"{compasMetric}/4 or {compasMetric}/8 time"
                    },
                    count = response.Count,
                    genres = response
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetGenresByCompas: {ex}");
                return StatusCode(500, new { 
                    error = "A system error occurred. Please try again later." 
                });
            }
        }
    }
}