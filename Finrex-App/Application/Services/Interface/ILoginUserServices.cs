using Finrex_App.Application.DTOs;
using Finrex_App.Domain.Entities;

namespace Finrex_App.Application.Services.Interface;

public interface ILoginUserServices
{
    Task<bool> RegisterAsync( RegisterDTO registerDto );
    Task<bool> UserExistsAsync( string email );
    Task<string?> LoginAsync( LoginUserDto loginUserDto );
}