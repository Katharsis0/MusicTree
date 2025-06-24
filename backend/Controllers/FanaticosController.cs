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
                    name = c.Name,
                    password = c.Password,
                    country = c.Country,
                    avatar = c.Avatar
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