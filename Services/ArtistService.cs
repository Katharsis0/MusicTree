using MusicTree.Models.DTOs;
using MusicTree.Models.Entities;
using MusicTree.Repositories;
using MusicTree.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MusicTree.Services
{
    public class ArtistService
    {
        private readonly ArtistRepository _artistRepo;

        public ArtistService(ArtistRepository artistRepo)
        {
            _artistRepo = artistRepo;
        }

        public async Task<Artist> CreateArtistAsync(ArtistCreateDto dto)
        {
            // Validate name uniqueness
            if (await _artistRepo.ExistsByNameAsync(dto.Name))
                throw new ArgumentException("Artist name already exists.");

            // Generate ID (e.g., "C-1A2B3C4D5E6")
            var artist = new Artist
            {
                Id = $"A-{GenerateRandomId(12)}",
                Name = dto.Name,
                Biography = dto.Biography,
                IsActive = true,
                TimeStamp = DateTime.UtcNow,
            };

            await _artistRepo.AddAsync(artist);
            return artist;
        }

        public async Task<IEnumerable<Artist>> GetAllArtistAsync(bool includeInactive = false)
        {
            return await _artistRepo.GetAllAsync(includeInactive);
        }

        private string GenerateRandomId(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}