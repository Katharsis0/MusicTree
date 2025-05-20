using Microsoft.AspNetCore.Mvc;
using MusicTree.Models.DTOs;
using MusicTree.Services.Interfaces;
using MusicTree.Models;

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
                // Validate model state
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var cluster = await _clusterService.CreateClusterAsync(dto);
                return CreatedAtAction(nameof(CreateCluster), new { id = cluster.Id }, cluster);
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
        public async Task<IActionResult> GetAllClusters([FromQuery] bool includeInactive = false)
        {
            try
            {
                var clusters = await _clusterService.GetAllClustersAsync(includeInactive);
                
                var clusterList = clusters.ToList();
                if (!clusterList.Any())
                {
                    return Ok(new { message = "Cluster list is empty", clusters = clusterList });
                }

                // Format response according to requirements
                var response = clusterList.Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    isActive = c.IsActive,
                    creationDate = c.TimeStamp
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