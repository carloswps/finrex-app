using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Finrex_App.Core.DTOs;
using Finrex_App.Core.Entities;
using Finrex_App.Core.JwtGenerate;
using Finrex_App.Infra.Data;
using Finrex_App.Services.Interface;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Finrex_App.Services;

public class AuthService : IAuthServices
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;

    public AuthService( AppDbContext context, ILogger<AuthService> logger, IConfiguration configuration )
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
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

    public List<User> GetUsers()
    {
        var users = _context.Users.ToList();
        return users;
    }


    public async Task<string> LoginAsync( LoginUserDto loginUserDto )
    {
        var user = await _context.Users
            .FirstOrDefaultAsync( u => u.Email == loginUserDto.Email && u.Senha == loginUserDto.Senha );

        if ( user == null )
        {
            return null;
        }

        return GenerateToken( user );
    }

    public string GenerateToken( User user )
    {
        var key = Encoding.UTF8.GetBytes( _configuration[ "Jwt:Key" ] ??
                                          throw new InvalidOperationException( "Nenhuma chave encontrada" ) );
        var tokenConfig = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity( new Claim[]
            {
                new( "user", user.Email )
            } ),
            Expires = DateTime.UtcNow.AddDays( 7 ),
            SigningCredentials = new SigningCredentials( new SymmetricSecurityKey( key ),
                SecurityAlgorithms.HmacSha256Signature )
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken( tokenConfig );
        var TokenString = tokenHandler.WriteToken( token );
        return TokenString;
    }
}