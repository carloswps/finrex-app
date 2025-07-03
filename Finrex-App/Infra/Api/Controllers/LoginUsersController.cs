using Finrex_App.Core.DTOs;
using Finrex_App.Core.Entities;
using Finrex_App.Services;
using Finrex_App.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion( "1.0" )]
[Route( "api/v{version:apiVersion}/[controller]" )]
public class LoginUsersController : ControllerBase
{
    private readonly IAuthServices _authService;
    private readonly ILogger<LoginUsersController> _logger;

    public LoginUsersController( IAuthServices authService, ILogger<LoginUsersController> logger )
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost( "register" )]
    public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto )
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

    [HttpPost( "login" )]
    public async Task<IActionResult> Login([FromBody] LoginUserDto loginUserDto )
    {
        var token = await _authService.LoginAsync( loginUserDto );
        if ( token == null )
        {
            return Unauthorized( "Credenciais invalidas" );
        }

        return Ok( new { token } );
    }

    [Authorize]
    [ResponseCache( Duration = 120 )]
    [HttpGet( "usuarios-temporarios" )]
    public OkObjectResult GetAll()
    {
        var users = _authService.GetUsers();
        return Ok( users );
    }
}