using Microsoft.EntityFrameworkCore;
using MusicTree.Models.Entities;
using MusicTree.Models.DTOs;
using MusicTree.Models;
using System.Linq.Expressions;

namespace MusicTree.Repositories
{
    public class ArtistRepository
    {
        private readonly AppDbContext _context;

        public ArtistRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Artist> AddAsync(Artist artist)
        {
            await _context.Artists.AddAsync(artist);
            await _context.SaveChangesAsync();
            return artist;
        }

        public async Task<Artist?> GetByIdAsync(string id)
        {
            return await _context.Artists
                .Include(a => a.ArtistGenres)
                    .ThenInclude(ag => ag.Genre)
                .Include(a => a.ArtistSubgenres)
                    .ThenInclude(asg => asg.Genre)
                        .ThenInclude(g => g.ParentGenre)
                .FirstOrDefaultAsync(a => a.Id == id);
        }
        
        public async Task<IEnumerable<Artist>> GetAllAsync(bool includeInactive = false)
        {
            var query = _context.Artists.AsQueryable();
    
            if (!includeInactive)
            {
                query = query.Where(a => a.IsActive);
            }

            return await query
                .Include(a => a.ArtistGenres)
                .Include(a => a.ArtistSubgenres)
                .OrderBy(a => a.Name)
                .ToListAsync();
        }

        public async Task<Artist?> GetByNameAsync(string name)
        {
            return await _context.Artists
                .FirstOrDefaultAsync(a => a.Name == name);
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Artists.AnyAsync(a => a.Name == name);
        }

        public async Task<PagedResult<Artist>> GetPagedAsync(
            Expression<Func<Artist, bool>> filter,
            Expression<Func<Artist, object>> sortBy,
            bool descending,
            int pageNumber,
            int pageSize)
        {
            var query = _context.Artists
                .Include(a => a.ArtistGenres)
                .Include(a => a.ArtistSubgenres)
                .Where(filter);

            var totalCount = await query.CountAsync();

            if (descending)
            {
                query = query.OrderByDescending(sortBy);
            }
            else
            {
                query = query.OrderBy(sortBy);
            }

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Artist>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<int> CountAsync(Expression<Func<Artist, bool>> filter)
        {
            return await _context.Artists.CountAsync(filter);
        }

        public async Task<Artist> UpdateAsync(Artist artist)
        {
            _context.Artists.Update(artist);
            await _context.SaveChangesAsync();
            return artist;
        }

        public async Task<Dictionary<string, int>> GetArtistCountByCountryAsync()
        {
            return await _context.Artists
                .Where(a => a.IsActive)
                .GroupBy(a => a.OriginCountry)
                .Select(g => new { Country = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Country, x => x.Count);
        }

        // Genre relationship methods (for future user stories)
        public async Task<ArtistGenre?> GetArtistGenreAssociationAsync(string artistId, string genreId)
        {
            return await _context.Set<ArtistGenre>()
                .FirstOrDefaultAsync(ag => ag.ArtistId == artistId && ag.GenreId == genreId);
        }

        public async Task AddArtistGenreAssociationAsync(string artistId, string genreId, float influenceCoefficient)
        {
            var association = new ArtistGenre
            {
                ArtistId = artistId,
                GenreId = genreId,
                InfluenceCoefficient = influenceCoefficient
            };

            await _context.Set<ArtistGenre>().AddAsync(association);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> RemoveArtistGenreAssociationAsync(string artistId, string genreId)
        {
            var association = await GetArtistGenreAssociationAsync(artistId, genreId);
            if (association == null) return false;

            _context.Set<ArtistGenre>().Remove(association);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Genre>> GetArtistGenresAsync(string artistId)
        {
            return await _context.Set<ArtistGenre>()
                .Where(ag => ag.ArtistId == artistId)
                .Include(ag => ag.Genre)
                .Select(ag => ag.Genre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Artist>> GetArtistsByGenreAsync(string genreId)
        {
            return await _context.Set<ArtistGenre>()
                .Where(ag => ag.GenreId == genreId)
                .Include(ag => ag.Artist)
                .Select(ag => ag.Artist)
                .ToListAsync();
        }
    }
}