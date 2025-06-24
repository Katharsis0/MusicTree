using Microsoft.EntityFrameworkCore;
using MusicTree.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using MusicTree.Models.DTOs;
using MusicTree.Models;

namespace MusicTree.Repositories
{
    public class FanaticoRepository
    {
        private readonly AppDbContext _context;

        public FanaticoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Fanatico fanatico)
        {
            await _context.Fanaticos.AddAsync(fanatico);
            await _context.SaveChangesAsync();
        }
        
        public async Task UpdateAsync(Fanatico fanatico)
        {
            _context.Fanaticos.Update(fanatico);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            return await _context.Fanaticos.AnyAsync(c => c.Username == username);
        }
        
        public async Task<OperationResult> ValidateAndAddAsync(FanaticoCreateDto dto)
        {
            if (await ExistsByUsernameAsync(dto.Username))
            {
                return OperationResult.CreateFailure("Fanatico with this username already exists");
            }
        
            var fanatico = new Fanatico
            {
                Name = dto.Name,
                Password = dto.Password,
                Username = dto.Username,
                Country = dto.Country,
                //Genres = dto.FanaticoRelatedGenres,
                Avatar = dto.Avatar,
                
            };
        
            await AddAsync(fanatico);
            return OperationResult.CreateSuccess(fanatico.Username);
        }
        public async Task<IEnumerable<Fanatico>> GetAllAsync()
        {
            var query = _context.Fanaticos.AsQueryable();
            return await query.OrderByDescending(c => c.Username).ToListAsync();
        }

    }
}