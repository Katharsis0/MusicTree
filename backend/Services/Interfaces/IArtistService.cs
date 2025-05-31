using MusicTree.Models.DTOs;
using MusicTree.Models.Entities;

namespace MusicTree.Services.Interfaces
{
    public interface IClusterArtist
    {
        Task<IEnumerable<Artist>> GetAllArtistAsync(bool includeInactive = false);
        Task<Artist> CreateArtistAsync(ArtistCreateDto dto);
    }
}