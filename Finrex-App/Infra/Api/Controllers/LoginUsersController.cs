using Finrex_App.Application.DTOs;
using Finrex_App.Application.Validators;
using Finrex_App.Core.DTOs;
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
    private readonly RegisterDTOValidator _dtoValidator;
    
    public LoginUsersController( IAuthServices authService, ILogger<LoginUsersController> logger, RegisterDTOValidator dtoValidator )
    {
        _authService = authService;
        _logger = logger;
        _dtoValidator = dtoValidator;
    }

    [HttpPost( "register" )]
    public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto )
    {
        try
        {
            var validationResult = await _dtoValidator.ValidateAsync( registerDto );
            if ( !validationResult.IsValid )
            {
                var errors = validationResult.Errors.Select( e => new
                {
                    Campo = e.PropertyName,
                    Mensagem = e.ErrorMessage,
                });
                return BadRequest(new
                {
                    Sucesso = false,
                    Erros = errors
                });
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