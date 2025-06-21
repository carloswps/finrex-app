using Finrex_App.DTOS;
using Finrex_App.Entities;
using Finrex_App.Infra;
using Microsoft.EntityFrameworkCore;

namespace Finrex_App.Services;

public class CriarUsuarioTeste
{
    private readonly AppDbContext _context;

    public CriarUsuarioTeste( AppDbContext context )
    {
        _context = context;
    }

    public async Task<bool> CriarUsuarioAsync( LoginDto loginDto )
    {
        if ( await _context.Contatos.AnyAsync( x => x.Email == loginDto.email ) )
        {
            return false; // Usuário já existe
        }

        var criarUsuario = new LoginDb
        {
            Email = loginDto.email,
            Senha = loginDto.senha
        };

        _context.Add( criarUsuario );
        await _context.SaveChangesAsync();
        return true;
    }
}