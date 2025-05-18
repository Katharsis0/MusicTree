using Microsoft.AspNetCore.Mvc;
using MusicTree.Models.DTOs;
using MusicTree.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MusicTree.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClustersController : ControllerBase
    {
        private readonly IClusterService _clusterService;

        public ClustersController(IClusterService clusterService)
        {
            _clusterService = clusterService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCluster([FromBody] ClusterCreateDto dto)
        {
            try
            {
                var cluster = await _clusterService.CreateClusterAsync(dto);
                return CreatedAtAction(null, new { id = cluster.Id }, cluster);
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

        [HttpGet]
        public async Task<IActionResult> GetAllClusters([FromQuery] bool includeInactive = false)
        {
            try
            {
                var clusters = await _clusterService.GetAllClustersAsync(includeInactive);
                return Ok(clusters);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}