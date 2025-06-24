using Finrex_App.Core.DTOs;
using Finrex_App.Core.Entities;
using Finrex_App.Infra.Data;
using Finrex_App.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace Finrex_App.Services;

public class AuthService : IAuthServices
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuthService> _logger;

    public AuthService( AppDbContext context, ILogger<AuthService> logger )
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> RegisterAsync( RegisterDTO registerDto )
    {
        try
        {
            if ( await _context.Users.AnyAsync( u => u.Email == registerDto.Email ) )
            {
                return false;
            }

            var user = new User
            {
                Nome = registerDto.Nome,
                Email = registerDto.Email,
                Senha = registerDto.Senha,
                CriadoEm = DateTime.UtcNow,
                AtualizadoEm = DateTime.UtcNow
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

    public async Task<List<User>> GetUserAsync()
    {
        try
        {
            return await _context.Users.ToListAsync();
        } catch ( Exception e )
        {
            _logger.LogError( e, "Error ao buscar usuarios" );
            throw;
        }
    }

    private string GenerateToken( User user )
    {
        // Implementar geração de JWT token
        return $"jwt_token_{user.Id}_{DateTime.UtcNow.Ticks}";
    }
}