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
    Console.WriteLine($"AddWithRelationsAsync called for genre: {genre.Name} (ID: {genre.Id})"); // Log entry

    try
    {
        await _context.Genres.AddAsync(genre);
        Console.WriteLine("Genre added to context, saving changes...");
        await _context.SaveChangesAsync(); // Save the genre first
        Console.WriteLine("Genre saved successfully.");

        if (relations != null && relations.Any())
        {
            Console.WriteLine($"Adding {relations.Count} relations...");
            var mgpcCalculator = new MgpcCalculator();
            foreach (var relation in relations)
            {
                Console.WriteLine($"Processing relation for GenreId: {relation.GenreId}");
                var relatedGenre = await GetByIdAsync(relation.GenreId);
                if (relatedGenre != null)
                {
                    Console.WriteLine($"Related genre found: {relatedGenre.Name} (ID: {relatedGenre.Id})");
                    var mgpc = mgpcCalculator.Calculate(genre, relatedGenre);
                    Console.WriteLine($"MGPC calculated: {mgpc}");
                    await AddGenreRelationAsync(genre.Id, relatedGenre.Id, relation.InfluenceStrength, mgpc);
                    Console.WriteLine("Relation added successfully.");
                }
                else
                {
                    Console.WriteLine($"Warning: Related Genre with ID {relation.GenreId} not found.");
                }
            }
        }
        Console.WriteLine("AddWithRelationsAsync completed."); // Log completion
    }
    catch (DbUpdateException dbEx)
    {
        Console.WriteLine($"DbUpdateException in AddWithRelationsAsync: {dbEx}");
        // Log inner exceptions for more details
        if (dbEx.InnerException != null)
        {
            Console.WriteLine($"Inner Exception: {dbEx.InnerException}");
        }
        throw;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Exception in AddWithRelationsAsync: {ex}");
        throw;
    }
}

public async Task AddGenreRelationAsync(string genreId, string relatedGenreId, int influence, float mgpc)
{
    Console.WriteLine($"AddGenreRelationAsync called: genreId={genreId}, relatedGenreId={relatedGenreId}, influence={influence}, mgpc={mgpc}");

    try
    {
        var relation = new GenreRelation
        {
            GenreId = genreId,
            RelatedGenreId = relatedGenreId,
            Influence = influence,
            MGPC = mgpc
        };

        _context.GenreRelations.Add(relation);
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
        throw;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Exception in AddGenreRelationAsync: {ex}");
        throw;
    }
}

        public async Task AddAsync(Genre genre)
        {
            await _context.Genres.AddAsync(genre);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByNameAsync(string name, string? parentGenreId = null)
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

        public async Task<Genre?> GetByIdAsync(string id)
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

        public async Task<IEnumerable<Genre>> GetAllAsync()
        {
            return await _context.Genres
                .Include(g => g.ParentGenre)
                .Include(g => g.Cluster)
                .Where(g => g.IsActive)
                .OrderBy(g => g.Name) // Alphabetical order as per user story
                .ToListAsync();
        }

        public async Task<IEnumerable<Genre>> GetMainGenresAsync()
        {
            return await _context.Genres
                .Include(g => g.Cluster)
                .Where(g => g.IsActive && !g.IsSubgenre)
                .OrderBy(g => g.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Genre>> GetSubgenresAsync(string? parentGenreId = null)
        {
            var query = _context.Genres
                .Include(g => g.ParentGenre)
                .Where(g => g.IsActive && g.IsSubgenre);

            if (!string.IsNullOrEmpty(parentGenreId))
            {
                query = query.Where(g => g.ParentGenreId == parentGenreId);
            }

            return await query.OrderBy(g => g.Name).ToListAsync();
        }
    }
}