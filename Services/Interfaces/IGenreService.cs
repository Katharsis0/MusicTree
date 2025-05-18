using MusicTree.Models.DTOs;
using MusicTree.Models.Entities;
using System.Threading.Tasks;

namespace MusicTree.Services.Interfaces
{
    public interface IGenreService
    {
        Task<Genre> CreateGenreAsync(GenreCreateDto dto);
        float CalculateMGPC(Genre genreA, Genre genreB);
    }
}