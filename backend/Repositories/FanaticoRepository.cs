using Microsoft.EntityFrameworkCore;
using MusicTree.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using MusicTree.Models.DTOs;
using MusicTree.Models;
using MusicTree.Models.Responses;

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
        public async Task CreateCalificacionAsync(string Username, string ArtistID, int Calificacion )
        {
            var fanaticocalificar = new FanaticoCalificacion
            {
                Username = Username,
                ArtistId = ArtistID,
                CalificacionId = Calificacion
            };
            
            await _context.Set<FanaticoCalificacion>().AddAsync(fanaticocalificar);
            await _context.SaveChangesAsync();

        }
        public async Task<IEnumerable<FanaticoCalificacion>> GetCalificacionesPorArtistaAsync(string artistId)
        {
            return await _context.FanaticosCalificacion
                .Where(c => c.ArtistId == artistId)
                .Select(c => new FanaticoCalificacion()
                {
                    Username = c.Username,
                    CalificacionId = c.CalificacionId
                })
                .ToListAsync();
        }

    }
}