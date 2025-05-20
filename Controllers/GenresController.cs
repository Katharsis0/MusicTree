using Microsoft.AspNetCore.Mvc;
using MusicTree.Models.DTOs;
using MusicTree.Services.Interfaces;

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

        [HttpPost]
        public async Task<IActionResult> CreateGenre([FromBody] GenreCreateDto dto)
        {
            try
            {
                // Validate model state
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate additional business rules
                if (dto.IsSubgenre && string.IsNullOrEmpty(dto.ParentGenreId))
                {
                    return BadRequest(new { message = "Parent genre is required for subgenres." });
                }

                if (dto.IsSubgenre && !string.IsNullOrEmpty(dto.ClusterId))
                {
                    return BadRequest(new { message = "Subgenres cannot be associated with clusters directly." });
                }

                var genre = await _genreService.CreateGenreAsync(dto);
                return CreatedAtAction(nameof(CreateGenre), new { id = genre.Id }, new
                {
                    id = genre.Id,
                    name = genre.Name,
                    description = genre.Description,
                    isSubgenre = genre.IsSubgenre,
                    parentGenreId = genre.ParentGenreId,
                    clusterId = genre.ClusterId,
                    creationDate = genre.TimeStamp
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error occurred while processing the request. Please try again later." });
            }
        }
    }
}