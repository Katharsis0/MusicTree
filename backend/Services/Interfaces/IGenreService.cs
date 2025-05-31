using MusicTree.Models.DTOs;
using MusicTree.Models.Entities;

namespace MusicTree.Services.Interfaces
{
    public interface IGenreService
    {
        Task<Genre> CreateGenreAsync(GenreCreateDto dto);
        Task<Genre?> GetGenreByIdAsync(string id);
        Task<IEnumerable<Genre>> GetAllGenresAsync();
        float CalculateMGPC(Genre genreA, Genre genreB);
    }
}