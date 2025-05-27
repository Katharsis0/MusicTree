using Microsoft.EntityFrameworkCore;
using MusicTree.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using MusicTree.Models.DTOs;
using MusicTree.Models;

namespace MusicTree.Repositories
{
    public class ArtistRepository
    {
        private readonly AppDbContext _context;

        public ArtistRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Artist artist)
        {
            await _context.Artists.AddAsync(artist);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Artists.AnyAsync(c => c.Name == name);
        }
        
        public async Task<OperationResult> ValidateAndAddAsync(ArtistCreateDto dto)
        {
            if (await ExistsByNameAsync(dto.Name))
            {
                return OperationResult.CreateFailure("Artist with this name already exists");
            }
        
            var artist = new Artist
            {
                Name = dto.Name,
                Biography = dto.Biography
            };
        
            await AddAsync(artist);
            return OperationResult.CreateSuccess(artist.Id);
        }

        public async Task<IEnumerable<Artist>> GetAllAsync(bool includeInactive = false)
        {
            var query = _context.Artists.AsQueryable();
            
            if (!includeInactive)
            {
                query = query.Where(c => c.IsActive);
            }

            return await query.OrderByDescending(c => c.Name).ToListAsync();
        }

        public async Task<Artist?> GetByIdAsync(string id)
        {
            return await _context.Artists
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}