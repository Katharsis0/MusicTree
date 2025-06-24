using Microsoft.AspNetCore.Mvc;
using MusicTree.Services.Interfaces;
namespace MusicTree.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FanaticoController : ControllerBase
{
    private readonly IFanaticoService _fanaticoService;

    public FanaticoController(IFanaticoService fanaticoService)
    {
        _fanaticoService = fanaticoService;
    }

    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar([FromBody] FanUserRegisterDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new OperationResult
                {
                    Success = false,
                    Message = "Por favor complete todos los campos correctamente.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var result = await _fanaticoService.RegistrarFanaticoAsync(dto);

            if (!result.Success)
                return Conflict(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en registro de fanático: {ex}");
            return StatusCode(500, new OperationResult
            {
                Success = false,
                Message = "Error inesperado. Intente nuevamente más tarde."
            });
        }
    }
}
