using MusicTree.Models.DTOs;
using MusicTree.Models.Entities;
using MusicTree.Repositories;
using MusicTree.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MusicTree.Services
{
    public class ClusterService : IClusterService
    {
        private readonly ClusterRepository _clusterRepo;

        public ClusterService(ClusterRepository clusterRepo)
        {
            _clusterRepo = clusterRepo;
        }

        public async Task<Cluster> CreateClusterAsync(ClusterCreateDto dto)
        {
            // Validate name uniqueness
            if (await _clusterRepo.ExistsByNameAsync(dto.Name))
                throw new ArgumentException("Cluster name already exists.");

            // Generate ID (e.g., "C-1A2B3C4D5E6")
            var cluster = new Cluster
            {
                Id = $"C-{GenerateRandomId(12)}",
                Name = dto.Name,
                Description = dto.Description,
                IsActive = dto.IsActive,
                TimeStamp = DateTime.UtcNow,
                Genres = new List<Genre>()
            };

            await _clusterRepo.AddAsync(cluster);
            return cluster;
        }

        public async Task<IEnumerable<Cluster>> GetAllClustersAsync(bool includeInactive = false)
        {
            return await _clusterRepo.GetAllAsync(includeInactive);
        }
        
        // Add method to toggle cluster status
        public async Task<bool> ToggleClusterStatusAsync(string clusterId)
        {
            var cluster = await _clusterRepo.GetByIdAsync(clusterId);
            if (cluster == null)
                return false;

            cluster.IsActive = !cluster.IsActive;
            await _clusterRepo.UpdateAsync(cluster);
            return true;
        }

        // Add method to set specific status
        public async Task<bool> SetClusterStatusAsync(string clusterId, bool isActive)
        {
            var cluster = await _clusterRepo.GetByIdAsync(clusterId);
            if (cluster == null)
                return false;

            cluster.IsActive = isActive;
            await _clusterRepo.UpdateAsync(cluster);
            return true;
        }

        private string GenerateRandomId(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}