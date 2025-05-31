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

        public async Task AddGenreRelationAsync(string genreId, string relatedGenreId, int influence, float mgpc)
        {
            Console.WriteLine($"AddGenreRelationAsync called: genreId={genreId}, relatedGenreId={relatedGenreId}, influence={influence}, mgpc={mgpc}");

            try
            {
                // Check if the relationship already exists
                var existingRelation = await _context.GenreRelations
                    .FirstOrDefaultAsync(gr => gr.GenreId == genreId && gr.RelatedGenreId == relatedGenreId);

                if (existingRelation != null)
                {
                    Console.WriteLine("Relationship already exists, updating values...");
                    existingRelation.Influence = influence;
                    existingRelation.MGPC = mgpc;
                    _context.GenreRelations.Update(existingRelation);
                }
                else
                {
                    Console.WriteLine("Creating new relationship...");
                    var relation = new GenreRelation
                    {
                        GenreId = genreId,
                        RelatedGenreId = relatedGenreId,
                        Influence = influence,
                        MGPC = mgpc
                    };

                    await _context.GenreRelations.AddAsync(relation);
                }

                await _context.SaveChangesAsync();
                Console.WriteLine("GenreRelation saved successfully.");
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"DbUpdateException in AddGenreRelationAsync: {dbEx}");
                if (dbEx.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {dbEx.InnerException}");
                }
                throw new InvalidOperationException($"Failed to save genre relationship: {dbEx.Message}", dbEx);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in AddGenreRelationAsync: {ex}");
                throw new InvalidOperationException($"Unexpected error saving genre relationship: {ex.Message}", ex);
            }
        }

        public async Task AddAsync(Genre genre)
        {
            try
            {
                Console.WriteLine($"Adding genre to context: {genre.Name} (ID: {genre.Id})");
                await _context.Genres.AddAsync(genre);
                await _context.SaveChangesAsync();
                Console.WriteLine("Genre saved successfully.");
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"DbUpdateException in AddAsync: {dbEx}");
                if (dbEx.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {dbEx.InnerException}");
                }
                throw new InvalidOperationException($"Failed to save genre: {dbEx.Message}", dbEx);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in AddAsync: {ex}");
                throw new InvalidOperationException($"Unexpected error saving genre: {ex.Message}", ex);
            }
        }

        public async Task<bool> ExistsByNameAsync(string name, string? parentGenreId = null)
        {
            try
            {
                if (parentGenreId == null)
                {
                    // Check for main genres (no parent)
                    return await _context.Genres
                        .Where(g => g.Name == name && !g.IsSubgenre)
                        .AnyAsync();
                }
                else
                {
                    // Check for subgenres under specific parent
                    return await _context.Genres
                        .Where(g => g.Name == name && g.IsSubgenre && g.ParentGenreId == parentGenreId)
                        .AnyAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ExistsByNameAsync: {ex}");
                throw new InvalidOperationException($"Failed to check genre name existence: {ex.Message}", ex);
            }
        }

        public async Task<Genre?> GetByIdAsync(string id)
        {
            try
            {
                return await _context.Genres
                    .Include(g => g.ParentGenre)
                    .Include(g => g.Cluster)
                    .Include(g => g.RelatedGenresAsSource)
                        .ThenInclude(r => r.RelatedGenre)
                    .Include(g => g.RelatedGenresAsTarget)
                        .ThenInclude(r => r.Genre)
                    .FirstOrDefaultAsync(g => g.Id == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetByIdAsync: {ex}");
                throw new InvalidOperationException($"Failed to retrieve genre: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Genre>> GetAllAsync()
        {
            try
            {
                return await _context.Genres
                    .Include(g => g.ParentGenre)
                    .Include(g => g.Cluster)
                    .Include(g => g.RelatedGenresAsSource)
                        .ThenInclude(r => r.RelatedGenre)
                    .Where(g => g.IsActive)
                    .OrderBy(g => g.Name) // Alphabetical order as per user story
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllAsync: {ex}");
                throw new InvalidOperationException($"Failed to retrieve genres: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Genre>> GetMainGenresAsync()
        {
            try
            {
                return await _context.Genres
                    .Include(g => g.Cluster)
                    .Include(g => g.RelatedGenresAsSource)
                        .ThenInclude(r => r.RelatedGenre)
                    .Where(g => g.IsActive && !g.IsSubgenre)
                    .OrderBy(g => g.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetMainGenresAsync: {ex}");
                throw new InvalidOperationException($"Failed to retrieve main genres: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Genre>> GetSubgenresAsync(string? parentGenreId = null)
        {
            try
            {
                var query = _context.Genres
                    .Include(g => g.ParentGenre)
                    .Include(g => g.RelatedGenresAsSource)
                        .ThenInclude(r => r.RelatedGenre)
                    .Where(g => g.IsActive && g.IsSubgenre);

                if (!string.IsNullOrEmpty(parentGenreId))
                {
                    query = query.Where(g => g.ParentGenreId == parentGenreId);
                }

                return await query.OrderBy(g => g.Name).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetSubgenresAsync: {ex}");
                throw new InvalidOperationException($"Failed to retrieve subgenres: {ex.Message}", ex);
            }
        }

        // Method to get genres by compás metric
        public async Task<IEnumerable<Genre>> GetByCompasMetricAsync(int compasMetric)
        {
            try
            {
                return await _context.Genres
                    .Include(g => g.ParentGenre)
                    .Include(g => g.Cluster)
                    .Where(g => g.IsActive && g.CompasMetric == compasMetric)
                    .OrderBy(g => g.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetByCompasMetricAsync: {ex}");
                throw new InvalidOperationException($"Failed to retrieve genres by compás metric: {ex.Message}", ex);
            }
        }

        // Method to get genres by BPM range
        public async Task<IEnumerable<Genre>> GetByBpmRangeAsync(int? minBpm = null, int? maxBpm = null)
        {
            try
            {
                var query = _context.Genres
                    .Include(g => g.ParentGenre)
                    .Include(g => g.Cluster)
                    .Where(g => g.IsActive);

                if (minBpm.HasValue)
                {
                    query = query.Where(g => g.BpmUpper >= minBpm.Value);
                }

                if (maxBpm.HasValue)
                {
                    query = query.Where(g => g.BpmLower <= maxBpm.Value);
                }

                return await query.OrderBy(g => g.Name).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetByBpmRangeAsync: {ex}");
                throw new InvalidOperationException($"Failed to retrieve genres by BPM range: {ex.Message}", ex);
            }
        }

        // Method to update MGPC values for existing relationships
        public async Task UpdateGenreRelationMGPCAsync(string genreId, string relatedGenreId, float newMgpc)
        {
            try
            {
                var relation = await _context.GenreRelations
                    .FirstOrDefaultAsync(gr => gr.GenreId == genreId && gr.RelatedGenreId == relatedGenreId);

                if (relation != null)
                {
                    relation.MGPC = newMgpc;
                    _context.GenreRelations.Update(relation);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Updated MGPC for relationship {genreId} -> {relatedGenreId}: {newMgpc}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateGenreRelationMGPCAsync: {ex}");
                throw new InvalidOperationException($"Failed to update MGPC: {ex.Message}", ex);
            }
        }

        // Method to get all relationships for a genre
        public async Task<IEnumerable<GenreRelation>> GetGenreRelationshipsAsync(string genreId)
        {
            try
            {
                return await _context.GenreRelations
                    .Include(gr => gr.Genre)
                    .Include(gr => gr.RelatedGenre)
                    .Where(gr => gr.GenreId == genreId || gr.RelatedGenreId == genreId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetGenreRelationshipsAsync: {ex}");
                throw new InvalidOperationException($"Failed to retrieve genre relationships: {ex.Message}", ex);
            }
        }

        // Method to delete a genre relationship
        public async Task<bool> RemoveGenreRelationAsync(string genreId, string relatedGenreId)
        {
            try
            {
                var relation = await _context.GenreRelations
                    .FirstOrDefaultAsync(gr => gr.GenreId == genreId && gr.RelatedGenreId == relatedGenreId);

                if (relation != null)
                {
                    _context.GenreRelations.Remove(relation);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Removed relationship: {genreId} -> {relatedGenreId}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RemoveGenreRelationAsync: {ex}");
                throw new InvalidOperationException($"Failed to remove genre relationship: {ex.Message}", ex);
            }
        }
    }
}