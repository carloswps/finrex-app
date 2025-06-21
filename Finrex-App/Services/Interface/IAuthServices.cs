using Finrex_App.DTOS;

namespace Finrex_App.Services.Interface;

public interface IAuthServices
{
    Task<LoginResponseDto> LoginAsync( LoginResponseDto loginDto );
    Task<bool> RegisterAsync( LoginResponseDto loginDto );
}