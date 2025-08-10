using Finrex_App.Application.DTOs;
using Finrex_App.Application.JwtGenerate;
using Finrex_App.Application.Services.Interface;
using Finrex_App.Core.DTOs;
using Finrex_App.Domain.Entities;
using Finrex_App.Infra.Data;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace Finrex_App.Application.Services;

public class LoginUserService : ILoginUserServices
{
    private readonly AppDbContext _context;
    private readonly ILogger<LoginUserService> _logger;
    private readonly TokeService _tokeService;
    private readonly IMapper _mapper;

    public LoginUserService(
        AppDbContext context, ILogger<LoginUserService> logger, TokeService tokeService, IMapper mapper )
    {
        _context = context;
        _logger = logger;
        _tokeService = tokeService;
        _mapper = mapper;
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
            var user = _mapper.Map<User>( registerDto );

            user.Senha = BCrypt.Net.BCrypt.HashPassword( registerDto.Senha );

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
        return _mapper.Map<List<User>>( users );
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