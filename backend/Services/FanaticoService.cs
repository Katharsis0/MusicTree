using MusicTree.Models.DTOs;
using MusicTree.Models.Entities;
using MusicTree.Repositories;
using MusicTree.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MusicTree.Services
{
    public class FanaticoService : IFanaticoService
    {
        private readonly FanaticoRepository _fanaticoRepo;

        public FanaticoService(FanaticoRepository fanaticoRepo)
        {
            _fanaticoRepo = fanaticoRepo;
        }

        public async Task<Fanatico> CreateFanaticoAsync(FanaticoCreateDto dto)
        {
            // Validate name uniqueness
            if (await _fanaticoRepo.ExistsByUsernameAsync(dto.Username))
                throw new ArgumentException("Fanatico username already exists.");

            // Generate ID (e.g., "C-1A2B3C4D5E6")
            var fanatico = new Fanatico
            {
                Name = dto.Name,
                Password = dto.Password,
                Username = dto.Username,
                Country = dto.Country,
                Avatar = dto.Avatar
                //Genres = new List<Genre>()
            };

            await _fanaticoRepo.AddAsync(fanatico);
            return fanatico;
        }

        public async Task<IEnumerable<Fanatico>> GetAllFanaticosAsync()
        {
            return await _fanaticoRepo.GetAllAsync();
        }
    }
}