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

}
