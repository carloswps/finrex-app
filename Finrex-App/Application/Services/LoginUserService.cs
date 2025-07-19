using Finrex_App.Application.DTOs;
using Finrex_App.Application.JwtGenerate;
using Finrex_App.Application.Services.Interface;
using Finrex_App.Core.DTOs;
using Finrex_App.Domain.Entities;
using Finrex_App.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Finrex_App.Application.Services;

public class LoginUserService : ILoginUserServices
{
    private readonly AppDbContext _context;
    private readonly ILogger<LoginUserService> _logger;
    private readonly TokeService _tokeService;

    public LoginUserService( AppDbContext context, ILogger<LoginUserService> logger, TokeService tokeService )
    {
        _context = context;
        _logger = logger;
        _tokeService = tokeService;
    }

    public async Task<bool> RegisterAsync( RegisterDTO registerDto )
    {
        try
        {
            if ( await _context.Users.AnyAsync( u => u.Email == registerDto.Email ) )
            {
                return false;
            }

            var senhaUserHash = BCrypt.Net.BCrypt.HashPassword( registerDto.Senha );
            var user = new User
            {
                Email = registerDto.Email,
                Senha = senhaUserHash,
                CriadoEm = DateOnly.MinValue,
                AtualizadoEm = DateOnly.MinValue
            };

            _context.Users.Add( user );
            await _context.SaveChangesAsync();
            return true;
        } catch ( Exception ex )
        {
            _logger.LogError( ex, "Erro ao registrar usuário" );
            throw;
        }
    }

    public List<User> GetUsers()
    {
        var users = _context.Users.ToList();
        return users;
    }


    public async Task<string?> LoginAsync( LoginUserDto loginUserDto )
    {
        var user = await _context.Users
            .FirstOrDefaultAsync( u => u.Email == loginUserDto.Email );

        if ( user == null ) { return null; }

        var senhaOk = BCrypt.Net.BCrypt.Verify( loginUserDto.Senha, user.Senha );
        if ( !senhaOk )
        {
            return null;
        }

        return _tokeService.GenerateToken( user );
    }
}