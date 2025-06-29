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

}
