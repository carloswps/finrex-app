using Finrex_App.Core.DTOs;
using Finrex_App.Core.Entities;

namespace Finrex_App.Services.Interface;

public interface IAuthServices
{
    Task<bool> RegisterAsync( RegisterDTO registerDto );

    Task<List<User>> GetUserAsync();
}