using Microsoft.AspNetCore.Mvc;
using MusicTree.Models.DTOs;
using MusicTree.Models.Entities;
using MusicTree.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using MusicTree.Services;

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
                
        /// <summary>
        /// Import multiple genres from JSON file
        /// </summary>
        /// <param name="file">JSON file containing genres array</param>
        /// <returns>Import result with success and error counts</returns>
        [HttpPost("import")]
        public async Task<IActionResult> ImportGenres(IFormFile file)
        {
            try
            {
                // Validate file presence
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { 
                        error = "No se proporcionó archivo o el archivo está vacío",
                        details = "Debe proporcionar un archivo JSON válido con géneros para importar"
                    });
                }

                // Validate file type
                if (!IsValidJsonFile(file))
                {
                    return BadRequest(new { 
                        error = "El archivo debe ser exclusivamente JSON",
                        details = "Solo se aceptan archivos con extensión .json y Content-Type application/json"
                    });
                }

                // Check file size (reasonable limit for JSON)
                const int maxFileSizeBytes = 10 * 1024 * 1024; // 10MB
                if (file.Length > maxFileSizeBytes)
                {
                    return BadRequest(new { 
                        error = "El archivo es demasiado grande",
                        details = "El tamaño máximo permitido es 10MB"
                    });
                }

                // Process the import
                var importService = HttpContext.RequestServices.GetRequiredService<GenreImportService>();
                var result = await importService.ImportGenresFromJsonAsync(file);

                // Return appropriate response based on results
                if (result.ErrorRecords == 0)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"La carga ha terminado. {result.ImportedRecords} géneros importados exitosamente.",
                        data = new
                        {
                            totalRecords = result.TotalRecords,
                            importedRecords = result.ImportedRecords,
                            errorRecords = result.ErrorRecords,
                            processedAt = result.ProcessedAt,
                            archivedFile = result.ImportedFileName,
                            errorFile = result.ErrorFileName
                        }
                    });
                }
                else if (result.ImportedRecords > 0)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"La carga ha terminado. {result.ImportedRecords} registros importados, {result.ErrorRecords} registros con errores.",
                        data = new
                        {
                            totalRecords = result.TotalRecords,
                            importedRecords = result.ImportedRecords,
                            errorRecords = result.ErrorRecords,
                            processedAt = result.ProcessedAt,
                            archivedFile = result.ImportedFileName,
                            errorFile = result.ErrorFileName
                        },
                        errors = result.Errors.Take(10).Select(e => new { 
                            record = e.OriginalRecord?.nombre ?? "Unknown",
                            error = e.ErrorDescription 
                        }).ToList(),
                        note = result.Errors.Count > 10 ? $"Mostrando solo los primeros 10 errores de {result.Errors.Count} total. Ver archivo {result.ErrorFileName} para detalles completos." : null
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"No se pudieron importar géneros. {result.ErrorRecords} registros con errores.",
                        data = new
                        {
                            totalRecords = result.TotalRecords,
                            importedRecords = result.ImportedRecords,
                            errorRecords = result.ErrorRecords,
                            processedAt = result.ProcessedAt,
                            errorFile = result.ErrorFileName
                        },
                        errors = result.Errors.Take(5).Select(e => new { 
                            record = e.OriginalRecord?.nombre ?? "Unknown",
                            error = e.ErrorDescription 
                        }).ToList()
                    });
                }
            }
            catch (ArgumentException ex)
            {
                // Handle validation errors (file format, etc.)
                return BadRequest(new { 
                    error = ex.Message,
                    details = "Verifique que el archivo sea un JSON válido con el formato correcto"
                });
            }
            catch (InvalidOperationException ex)
            {
                // Handle business logic errors
                return StatusCode(500, new { 
                    error = ex.Message,
                    details = "Error durante el procesamiento masivo"
                });
            }
            catch (Exception ex)
            {
                // Log unexpected errors
                Console.WriteLine($"Unexpected error in ImportGenres: {ex}");
                return StatusCode(500, new { 
                    error = "Ocurrió un error durante el procesamiento masivo y solicitará intentarlo nuevamente más tarde.",
                    details = ex.Message // Remove this in production
                });
            }
        }

        /// <summary>
        /// Download error file from import process
        /// </summary>
        /// <param name="filename">Error file name</param>
        /// <returns>Error file content</returns>
        [HttpGet("import/errors/{filename}")]
        public async Task<IActionResult> DownloadErrorFile(string filename)
        {
            try
            {
                var archiveFolder = HttpContext.RequestServices
                    .GetRequiredService<IConfiguration>()
                    .GetValue<string>("ImportSettings:ArchiveFolder") ?? "Archives";
                    
                var filePath = Path.Combine(archiveFolder, filename);
                
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new { error = "Archivo de errores no encontrado" });
                }
                
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                return File(fileBytes, "application/json", filename);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading error file: {ex}");
                return StatusCode(500, new { error = "Error al descargar archivo de errores" });
            }
        }

        /// <summary>
        /// Get import history
        /// </summary>
        /// <returns>List of recent imports</returns>
        [HttpGet("import/history")]
        public async Task<IActionResult> GetImportHistory()
        {
            try
            {
                var archiveFolder = HttpContext.RequestServices
                    .GetRequiredService<IConfiguration>()
                    .GetValue<string>("ImportSettings:ArchiveFolder") ?? "Archives";
                    
                if (!Directory.Exists(archiveFolder))
                {
                    return Ok(new { imports = new List<object>() });
                }
                
                var files = Directory.GetFiles(archiveFolder, "*.json")
                    .Where(f => !Path.GetFileName(f).Contains("_Errores"))
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .Take(20)
                    .Select(f => new
                    {
                        filename = f.Name,
                        importDate = f.CreationTime,
                        size = f.Length,
                        errorFile = Directory.Exists(archiveFolder) && Directory.GetFiles(archiveFolder, f.Name.Replace(".json", "_Errores.json")).Any()
                    })
                    .ToList();
                    
                return Ok(new { imports = files });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting import history: {ex}");
                return StatusCode(500, new { error = "Error al obtener historial de importaciones" });
            }
        }

        private static bool IsValidJsonFile(IFormFile file)
        {
            return file.ContentType == "application/json" || 
                   Path.GetExtension(file.FileName).Equals(".json", StringComparison.OrdinalIgnoreCase);
        }
                
    }
}