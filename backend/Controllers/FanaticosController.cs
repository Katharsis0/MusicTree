using Microsoft.AspNetCore.Mvc;
using MusicTree.Models.DTOs;
using MusicTree.Services.Interfaces;
using MusicTree.Models;

namespace MusicTree.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FanaticosController : ControllerBase
    {
        private readonly IFanaticoService _fanaticoService;

        public FanaticosController(IFanaticoService fanaticoService)
        {
            _fanaticoService = fanaticoService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateFanatico([FromBody] FanaticoCreateDto dto)
        {
            try
            {
                // Validate model  
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var fanatico = await _fanaticoService.CreateFanaticoAsync(dto);
                //Console.WriteLine("Se logró");
                return CreatedAtAction(nameof(CreateFanatico), new { username = fanatico.Username }, fanatico);
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

        [HttpGet]
        public async Task<IActionResult> GetAllFanaticos()
        {
            try
            {
                var fanaticos = await _fanaticoService.GetAllFanaticosAsync();
                
                var fanaticoList = fanaticos.ToList();
                if (!fanaticoList.Any())
                {
                    return Ok(new { message = "Fanatico list is empty", fanaticos = fanaticoList });
                }

                // Format response according to requirements
                var response = fanaticoList.Select(c => new
                {
                    username = c.Username,
                    password = c.Password,
                }).ToList();

                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error occurred while processing the request. Please try again later." });
            }
        }
        [HttpPost("calificar")]
        public async Task<IActionResult> CalificarArtista([FromBody] FanaticoCalificarDto dto)
        {
            try
            {
                // Validate model  
                if (!ModelState.IsValid)
                {
                    
                    return BadRequest(ModelState);
                }

                await _fanaticoService.CreateCalificacionAsync(dto);
                return Ok(new { message = "Calificación registrada", artistaId = dto.ArtistID });

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
        
        
        
        [HttpGet("calificar/{artistId}")]
        public async Task<IActionResult> GetAllFanaticosCalificacions(string artistId)
        {
            try
            {
                var fanaticoscalificacions = await _fanaticoService.GetAllFanaticosPorArtistaAsync(artistId);
                
                var fanaticoList = fanaticoscalificacions.ToList();
                if (!fanaticoList.Any())
                {
                    return Ok(new { message = "Fanatico list is empty", fanaticos = fanaticoList });
                }

                // Format response according to requirements
                var response = fanaticoList.Select(c => new
                {
                    username = c.Username,
                    calificacion = c.CalificacionId,
                }).ToList();

                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error occurred while processing the request. Please try again later." });
            }
        }
    }
}