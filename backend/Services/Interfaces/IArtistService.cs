using MusicTree.Models.DTOs;
using MusicTree.Models.Entities;
using MusicTree.Models;

namespace MusicTree.Services.Interfaces
{
    public interface IArtistService
    {
        Task<Artist> CreateArtistAsync(ArtistCreateDto dto);
        Task<Artist?> GetArtistByIdAsync(string id);
        Task<PagedResult<Artist>> GetAllArtistsAsync(ArtistSearchParams searchParams);
        Task<Artist?> UpdateArtistAsync(string id, ArtistCreateDto dto);
        Task<bool> DeleteArtistAsync(string id);
        Task<bool> ReactivateArtistAsync(string id);
        
        // Legacy method
        Task<IEnumerable<Artist>> GetAllArtistAsync(bool includeInactive = false);
        
        // Genre relationship methods
        Task<bool> AssociateWithGenreAsync(string artistId, string genreId, float influenceCoefficient = 1.0f);
        Task<bool> RemoveGenreAssociationAsync(string artistId, string genreId);
        Task<IEnumerable<Genre>> GetArtistGenresAsync(string artistId);
        Task<IEnumerable<Artist>> GetArtistsByGenreAsync(string genreId);
        
        // Statistics methods
        Task<double> CalculateArtistDiversityScoreAsync(string artistId);
        Task<Dictionary<string, int>> GetArtistCountByCountryAsync();
    }
}