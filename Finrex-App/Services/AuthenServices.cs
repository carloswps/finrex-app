using Finrex_App.DTOS;
using Finrex_App.Entities;
using Finrex_App.Infra;
using Finrex_App.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace Finrex_App.Services;

public class AuthService : IAuthServices
{
    private readonly AppDbContext _context;

    public AuthService( AppDbContext context )
    {
        _context = context;
    }

    public async Task<LoginResponseDto?> LoginAsync( LoginResponseDto loginDto )
    {
        var user = await _context.Contatos
            .FirstOrDefaultAsync( x => x.Email == loginDto.email && x.Senha == loginDto.senha );

        if ( user == null )
        {
            return null;
        }

        return new LoginResponseDto
        {
            id = user.Id,
            email = user.Email,
            senha = GenerateToken( user )
        };
    }

    public async Task<bool> RegisterAsync( LoginResponseDto loginDto )
    {
        if ( await _context.Contatos.AnyAsync( x => x.Email == loginDto.email ) )
        {
            return false;
        }

        var newUser = new LoginDb
        {
            Email = loginDto.email,
            Senha = loginDto.senha
        };

        _context.Add( newUser );
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ValidateTokenAsync( string token )
    {
        return await Task.FromResult( true );
    }

    private string GenerateToken( LoginDb user )
    {
        // Implementar geração de JWT token
        return $"token_{user.Id}_{DateTime.UtcNow.Ticks}";
    }
}