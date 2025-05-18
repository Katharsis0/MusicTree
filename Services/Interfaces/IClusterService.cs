using MusicTree.Models.DTOs;
using MusicTree.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MusicTree.Services.Interfaces
{
    public interface IClusterService
    {
        Task<Cluster> CreateClusterAsync(ClusterCreateDto dto);
        Task<IEnumerable<Cluster>> GetAllClustersAsync(bool includeInactive = false);
    }
}