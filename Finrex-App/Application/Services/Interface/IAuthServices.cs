using Finrex_App.Application.DTOs;
using Finrex_App.Core.DTOs;
using Finrex_App.Domain.Entities;

namespace Finrex_App.Services.Interface;

public interface IAuthServices
{
    Task<bool> RegisterAsync( RegisterDTO registerDto );
    List<User> GetUsers();
    Task<string?> LoginAsync( LoginUserDto loginUserDto );
}