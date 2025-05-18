using Microsoft.AspNetCore.Mvc;
using MusicTree.Models.DTOs;
using MusicTree.Services.Interfaces;
using System;
using System.Threading.Tasks;

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
                var genre = await _genreService.CreateGenreAsync(dto);
                return CreatedAtAction(null, new { id = genre.Id }, genre);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}