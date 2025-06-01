// Services/GenreImportService.cs
using MusicTree.Models.DTOs;
using MusicTree.Models.Entities;
using MusicTree.Repositories;
using MusicTree.Utils;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace MusicTree.Services
{
    public class GenreImportService
    {
        private readonly GenreRepository _genreRepo;
        private readonly ILogger<GenreImportService> _logger;
        private readonly string _archiveFolder;

        public GenreImportService(
            GenreRepository genreRepo, 
            ILogger<GenreImportService> logger,
            IConfiguration configuration)
        {
            _genreRepo = genreRepo;
            _logger = logger;
            _archiveFolder = configuration.GetValue<string>("ImportSettings:ArchiveFolder") ?? "Archives";
            
            // Ensure archive folder exists
            if (!Directory.Exists(_archiveFolder))
            {
                Directory.CreateDirectory(_archiveFolder);
            }
        }

        public async Task<GenreImportResultDto> ImportGenresFromJsonAsync(IFormFile jsonFile)
        {
            _logger.LogInformation("Starting genre import from file: {FileName}", jsonFile.FileName);

            var result = new GenreImportResultDto();
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");

            try
            {
                // Validate file format
                if (!IsValidJsonFile(jsonFile))
                {
                    throw new ArgumentException("El archivo debe ser exclusivamente JSON");
                }

                // Read and parse JSON
                var jsonContent = await ReadFileContentAsync(jsonFile);
                var importData = await ParseJsonContentAsync(jsonContent);

                result.TotalRecords = importData.Count;

                // Process each genre
                var processedGenres = new List<Genre>();
                var errors = new List<GenreImportErrorDto>();

                foreach (var genreDto in importData)
                {
                    try
                    {
                        // Validate individual record
                        var validationErrors = ValidateGenreRecord(genreDto);
                        if (validationErrors.Any())
                        {
                            errors.Add(new GenreImportErrorDto
                            {
                                OriginalRecord = genreDto,
                                ErrorDescription = string.Join("; ", validationErrors),
                                FieldName = "Validation"
                            });
                            continue;
                        }

                        // Check business rules
                        var businessValidation = await ValidateBusinessRulesAsync(genreDto);
                        if (!businessValidation.IsValid)
                        {
                            errors.Add(new GenreImportErrorDto
                            {
                                OriginalRecord = genreDto,
                                ErrorDescription = businessValidation.ErrorMessage,
                                FieldName = businessValidation.FieldName
                            });
                            continue;
                        }

                        // Convert to entity and save
                        var genre = await ConvertToGenreEntityAsync(genreDto);
                        await _genreRepo.AddAsync(genre);
                        processedGenres.Add(genre);

                        result.ImportedRecords++;
                        _logger.LogDebug("Successfully imported genre: {GenreName}", genre.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing genre: {GenreName}", genreDto.nombre);
                        errors.Add(new GenreImportErrorDto
                        {
                            OriginalRecord = genreDto,
                            ErrorDescription = $"Error durante el procesamiento: {ex.Message}",
                            FieldName = "Processing"
                        });
                    }
                }

                result.ErrorRecords = errors.Count;
                result.Errors = errors;

                // Process relationships and calculate MGPC
                await ProcessGenreRelationshipsAsync(processedGenres, importData);

                // Archive files
                result.ImportedFileName = await ArchiveOriginalFileAsync(jsonFile, timestamp);
                result.ErrorFileName = await CreateErrorFileAsync(errors, timestamp);

                _logger.LogInformation(
                    "Import completed. Total: {Total}, Imported: {Imported}, Errors: {Errors}",
                    result.TotalRecords, result.ImportedRecords, result.ErrorRecords);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error during genre import");
                throw new InvalidOperationException("Ocurrió un error durante el procesamiento masivo. Inténtelo nuevamente más tarde.");
            }
        }

        private static bool IsValidJsonFile(IFormFile file)
        {
            return file.ContentType == "application/json" || 
                   Path.GetExtension(file.FileName).Equals(".json", StringComparison.OrdinalIgnoreCase);
        }

        private static async Task<string> ReadFileContentAsync(IFormFile file)
        {
            using var reader = new StreamReader(file.OpenReadStream());
            return await reader.ReadToEndAsync();
        }

        private static async Task<List<GenreImportDto>> ParseJsonContentAsync(string jsonContent)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true
                };

                var result = JsonSerializer.Deserialize<List<GenreImportDto>>(jsonContent, options);
                return result ?? new List<GenreImportDto>();
            }
            catch (JsonException ex)
            {
                throw new ArgumentException($"Invalid JSON format: {ex.Message}");
            }
        }

        private static List<string> ValidateGenreRecord(GenreImportDto dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResults = new List<ValidationResult>();
            var errors = new List<string>();

            if (!Validator.TryValidateObject(dto, validationContext, validationResults, true))
            {
                errors.AddRange(validationResults.Select(vr => vr.ErrorMessage ?? "Unknown validation error"));
            }

            return errors;
        }

        private async Task<(bool IsValid, string ErrorMessage, string FieldName)> ValidateBusinessRulesAsync(GenreImportDto dto)
        {
            // Check name uniqueness
            var existingGenre = await _genreRepo.ExistsByNameAsync(dto.nombre, dto.es_subgenero ? dto.genero_padre : null);
            if (existingGenre)
            {
                return (false, $"Ya existe un {(dto.es_subgenero ? "subgénero" : "género")} con el nombre '{dto.nombre}'", nameof(dto.nombre));
            }

            // Validate parent genre exists (for subgenres)
            if (dto.es_subgenero && !string.IsNullOrEmpty(dto.genero_padre))
            {
                var parentExists = await _genreRepo.GetByIdAsync(dto.genero_padre);
                if (parentExists == null)
                {
                    return (false, $"No se encontró el género padre con ID '{dto.genero_padre}'", nameof(dto.genero_padre));
                }

                if (parentExists.IsSubgenre)
                {
                    return (false, "No se puede crear un subgénero de otro subgénero", nameof(dto.genero_padre));
                }
            }

            // Validate related genres exist
            if (dto.generos_relacionados?.Any() == true)
            {
                foreach (var relation in dto.generos_relacionados)
                {
                    // For the import, we'll look up by name since the JSON uses names
                    var relatedGenre = await FindGenreByNameAsync(relation.nombre);
                    if (relatedGenre == null)
                    {
                        return (false, $"No se encontró el género relacionado '{relation.nombre}'", nameof(dto.generos_relacionados));
                    }
                }
            }

            return (true, string.Empty, string.Empty);
        }

        private async Task<Genre?> FindGenreByNameAsync(string genreName)
        {
            var allGenres = await _genreRepo.GetAllAsync();
            return allGenres.FirstOrDefault(g => g.Name.Equals(genreName, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<Genre> ConvertToGenreEntityAsync(GenreImportDto dto)
        {
            var genre = new Genre(dto.es_subgenero)
            {
                Name = dto.nombre,
                Description = dto.descripcion,
                IsSubgenre = dto.es_subgenero,
                ParentGenreId = dto.es_subgenero ? dto.genero_padre : null,
                Key = dto.tono_dominante,
                BpmLower = dto.bpm.min,
                BpmUpper = dto.bpm.max,
                Bpm = (dto.bpm.min + dto.bpm.max) / 2,
                GenreCreationYear = dto.anio_creacion,
                GenreOriginCountry = dto.pais_origen,
                GenreTipicalMode = dto.modo,
                Volume = dto.volumen_tipico_db,
                CompasMetric = dto.compas,
                AvrgDuration = dto.duracion_promedio_segundos,
                IsActive = dto.activo
            };

            // Set RGB color if provided (only for main genres)
            if (!dto.es_subgenero && !string.IsNullOrEmpty(dto.color))
            {
                var (r, g, b) = ParseRgbColor(dto.color);
                genre.SetRgbColor(r, g, b);
            }

            // Generate ID
            genre.SetId();

            return genre;
        }

        private static (int r, int g, int b) ParseRgbColor(string rgbColor)
        {
            // Parse rgb(r,g,b) format
            if (!rgbColor.StartsWith("rgb(") || !rgbColor.EndsWith(")"))
                throw new ArgumentException($"Invalid RGB color format: {rgbColor}");

            var rgbContent = rgbColor[4..^1]; // Remove "rgb(" and ")"
            var parts = rgbContent.Split(',');

            if (parts.Length != 3)
                throw new ArgumentException($"Invalid RGB color format: {rgbColor}");

            if (!int.TryParse(parts[0].Trim(), out int r) || r < 0 || r > 255)
                throw new ArgumentException($"Invalid red component in RGB color: {parts[0]}");

            if (!int.TryParse(parts[1].Trim(), out int g) || g < 0 || g > 255)
                throw new ArgumentException($"Invalid green component in RGB color: {parts[1]}");

            if (!int.TryParse(parts[2].Trim(), out int b) || b < 0 || b > 255)
                throw new ArgumentException($"Invalid blue component in RGB color: {parts[2]}");

            return (r, g, b);
        }

        private async Task ProcessGenreRelationshipsAsync(List<Genre> processedGenres, List<GenreImportDto> importData)
        {
            var mgpcCalculator = new MgpcCalculator();

            foreach (var processedGenre in processedGenres)
            {
                var dto = importData.First(d => d.nombre == processedGenre.Name);

                // Handle subgenre-parent relationship
                if (dto.es_subgenero && !string.IsNullOrEmpty(dto.genero_padre))
                {
                    var parentGenre = await _genreRepo.GetByIdAsync(dto.genero_padre);
                    if (parentGenre != null)
                    {
                        var mgpc = mgpcCalculator.Calculate(processedGenre, parentGenre);
                        await _genreRepo.AddGenreRelationAsync(processedGenre.Id, parentGenre.Id, 10, mgpc);
                    }
                }

                // Handle explicit relationships
                if (dto.generos_relacionados?.Any() == true)
                {
                    foreach (var relation in dto.generos_relacionados)
                    {
                        var relatedGenre = await FindGenreByNameAsync(relation.nombre);
                        if (relatedGenre != null)
                        {
                            var mgpc = mgpcCalculator.Calculate(processedGenre, relatedGenre);
                            await _genreRepo.AddGenreRelationAsync(
                                processedGenre.Id, 
                                relatedGenre.Id, 
                                relation.influencia, 
                                mgpc);
                        }
                    }
                }
            }
        }

        private async Task<string> ArchiveOriginalFileAsync(IFormFile file, string timestamp)
        {
            var archivedFileName = $"{timestamp}_{file.FileName}";
            var archivePath = Path.Combine(_archiveFolder, archivedFileName);

            using var fileStream = new FileStream(archivePath, FileMode.Create);
            await file.CopyToAsync(fileStream);

            _logger.LogInformation("Original file archived as: {ArchivedFileName}", archivedFileName);
            return archivedFileName;
        }

        private async Task<string> CreateErrorFileAsync(List<GenreImportErrorDto> errors, string timestamp)
        {
            var errorFileName = $"{timestamp}_Errores.json";
            var errorPath = Path.Combine(_archiveFolder, errorFileName);

            var errorData = errors.Select(e => new
            {
                original_record = e.OriginalRecord,
                error_description = e.ErrorDescription,
                field_name = e.FieldName
            });

            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var jsonContent = JsonSerializer.Serialize(errorData, jsonOptions);
            await File.WriteAllTextAsync(errorPath, jsonContent);

            _logger.LogInformation("Error file created: {ErrorFileName}", errorFileName);
            return errorFileName;
        }
    }
}