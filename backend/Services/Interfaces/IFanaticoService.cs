using MusicTree.Models.DTOs;
using MusicTree.Models.Entities;

namespace MusicTree.Services.Interfaces
{
    public interface IFanaticoService
    {
        Task<Fanatico> CreateFanaticoAsync(FanaticoCreateDto dto);
        Task<IEnumerable<Fanatico>> GetAllFanaticosAsync();
        Task CreateCalificacionAsync(FanaticoCalificarDto dto);
        Task<IEnumerable<FanaticoCalificacion>> GetAllFanaticosPorArtistaAsync(string artistId);
        
    }
}
