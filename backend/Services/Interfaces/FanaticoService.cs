using Microsoft.AspNetCore.Identity;
using MusicTree.Models.DTOs;
using MusicTree.Models.Entities;
using MusicTree.Models.Responses;
using MusicTree.Repositories;
using MusicTree.Services.Interfaces;

public class FanaticoService : IFanaticoService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<FanUser> _hasher;

    public FanaticoService(AppDbContext context, IPasswordHasher<FanUser> hasher)
    {
        _context = context;
        _hasher = hasher;
    }

    public async Task<OperationResult> RegistrarFanaticoAsync(FanUserRegisterDto dto)
    {
        if (_context.Fanaticos.Any(f => f.Nickname == dto.Nickname))
        {
            return new OperationResult
            {
                Success = false,
                Message = "El nombre de usuario ya existe."
            };
        }

        var fan = new FanUser
        {
            Nickname = dto.Nickname,
            Nombre = dto.Nombre,
            Pais = dto.Pais,
            Avatar = dto.Avatar,
            PasswordHash = _hasher.HashPassword(null, dto.Password),
            GenerosFavoritos = dto.GenerosFavoritos.Select(id => new FanUserGenero { GeneroId = id }).ToList()
        };

        _context.Fanaticos.Add(fan);
        await _context.SaveChangesAsync();

        return new OperationResult
        {
            Success = true,
            Message = "Fanático registrado exitosamente"
        };
    }
}
