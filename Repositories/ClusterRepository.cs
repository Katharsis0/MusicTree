using Microsoft.EntityFrameworkCore;
using MusicTree.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using MusicTree.Models.DTOs;
using MusicTree.Models;

namespace MusicTree.Repositories
{
    public class ClusterRepository
    {
        private readonly AppDbContext _context;

        public ClusterRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Cluster cluster)
        {
            await _context.Clusters.AddAsync(cluster);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Clusters.AnyAsync(c => c.Name == name);
        }
        
        public async Task<OperationResult> ValidateAndAddAsync(ClusterCreateDto dto)
        {
            if (await ExistsByNameAsync(dto.Name))
            {
                return OperationResult.CreateFailure("Cluster with this name already exists");
            }
        
            var cluster = new Cluster
            {
                Name = dto.Name,
                Description = dto.Description
                // IsActive and TimeStamp are set by default
            };
        
            await AddAsync(cluster);
            return OperationResult.CreateSuccess(cluster.Id);
        }

        public async Task<IEnumerable<Cluster>> GetAllAsync(bool includeInactive = false)
        {
            var query = _context.Clusters.AsQueryable();
            
            if (!includeInactive)
            {
                query = query.Where(c => c.IsActive);
            }

            return await query.OrderByDescending(c => c.Name).ToListAsync();
        }

        public async Task<Cluster?> GetByIdAsync(string id)
        {
            return await _context.Clusters
                .Include(c => c.Genres)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}