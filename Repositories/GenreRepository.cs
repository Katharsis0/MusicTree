using MusicTree.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MusicTree.Repositories
{
    public class GenreRepository
    {
        private readonly AppDbContext _context;

        public GenreRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Genre genre)
        {
            await _context.Genres.AddAsync(genre);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByNameAsync(string name, string? parentGenreId = null)
        {
            return await _context.Genres.AnyAsync(g => 
                g.Name == name && 
                (parentGenreId == null || g.ParentGenre.Id == parentGenreId));
        }

        public async Task<Genre?> GetByIdAsync(string id)
        {
            return await _context.Genres
                .Include(g => g.ParentGenre)
                .Include(g => g.Cluster)
                .Include(g => g.RelatedGenres)
                .FirstOrDefaultAsync(g => g.Id == id);
        }
    }
}