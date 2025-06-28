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
            }

            if (!result.Success)
                return Conflict(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en registro de fanático: {ex}");
        }
    }
}
