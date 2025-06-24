using Finrex_App.Core.DTOs;
using Finrex_App.Core.Entities;
using Finrex_App.Services;
using Finrex_App.Services.Interface;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route( "api/[controller]" )]
public class AuthController : ControllerBase
{
    private readonly IAuthServices _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController( IAuthServices authService, ILogger<AuthController> logger )
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpGet( "users" )]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var users = await _authService.GetUserAsync();
            return Ok( users );
        } catch ( Exception e )
        {
            _logger.LogError( e, "Erro ao buscar usuários" );
            return StatusCode( 500, "Error interno na aplicação" );
        }
    }

    [HttpPost( "register" )]
    public async Task<IActionResult> Register( RegisterDTO registerDto )
    {
        try
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }

            var result = await _authService.RegisterAsync( registerDto );

            if ( !result )
            {
                return BadRequest( "Não foi possivel realizar o cadastro" );
            }

            return CreatedAtAction( nameof( Register ), new { email = registerDto.Email },
                "Usuario cadastrado com sucesso" );
        } catch ( Exception e )
        {
            _logger.LogError( e, "Erro ao realizar cadastro" );
            throw;
        }
    }
}