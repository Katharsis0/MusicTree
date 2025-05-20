using MusicTree.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MusicTree.Models.DTOs;
using MusicTree.Utils;

namespace MusicTree.Repositories
{
    public class GenreRepository
    {
        private readonly AppDbContext _context;

        public GenreRepository(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task AddWithRelationsAsync(Genre genre, List<GenreRelationDto>? relations)
        {
            await _context.Genres.AddAsync(genre);
        
            if (relations != null && relations.Any())
            {
                var mgpcCalculator = new MgpcCalculator();
                foreach (var relation in relations)
                {
                    var relatedGenre = await GetByIdAsync(relation.GenreId);
                    if (relatedGenre != null)
                    {
                        var mgpc = mgpcCalculator.Calculate(genre, relatedGenre);
                        _context.GenreRelations.Add(new GenreRelation
                        {
                            GenreId = genre.Id,
                            RelatedGenreId = relatedGenre.Id,
                            Influence = relation.InfluenceStrength,
                            MGPC = mgpc
                        });
                    }
                }
            }
        
            await _context.SaveChangesAsync();
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