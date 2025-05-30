using MusicTree.Models.DTOs;
using MusicTree.Models.Entities;

namespace MusicTree.Services.Interfaces
{
    public interface IClusterService
    {
        Task<Cluster> CreateClusterAsync(ClusterCreateDto dto);
        Task<IEnumerable<Cluster>> GetAllClustersAsync(bool includeInactive = false);
        Task<bool> ToggleClusterStatusAsync(string clusterId);
        Task<bool> SetClusterStatusAsync(string clusterId, bool isActive);
    }
}