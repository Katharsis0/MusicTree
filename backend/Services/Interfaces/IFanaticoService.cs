using MusicTree.Models.DTOs;
using MusicTree.Models.Responses;

namespace MusicTree.Services.Interfaces;

public interface IFanaticoService
{
    Task<OperationResult> RegistrarFanaticoAsync(FanUserRegisterDto dto);
}
