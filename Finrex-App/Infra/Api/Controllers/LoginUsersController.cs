using Finrex_App.Application.DTOs;
using Finrex_App.Application.Services.Interface;
using Finrex_App.Application.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finrex_App.Infra.Api.Controllers;

[ApiController]
[ApiVersion( "1.0" )]
[Route( "api/v{version:apiVersion}/[controller]" )]
public class LoginUsersController : ControllerBase
{
    private readonly ILoginUserServices _loginUserService;
    private readonly ILogger<LoginUsersController> _logger;
    private readonly RegisterDTOValidator _dtoValidator;

    public LoginUsersController(
        ILoginUserServices loginUserService, ILogger<LoginUsersController> logger, RegisterDTOValidator dtoValidator )
    {
        _loginUserService = loginUserService;
        _logger = logger;
        _dtoValidator = dtoValidator;
    }

    /// <summary>
    /// Realiza o cadastro de um novo usuário no sistema.
    /// </summary>
    /// <param name="registerDto">Dados necessários para o cadastro do usuário.</param>
    /// <returns>Retorna o status do cadastro.</returns>
    [HttpPost( "register" )]
    public async Task<IActionResult> Register( [FromBody] RegisterDTO registerDto )
    {
        try
        {
            var validationResult = await _dtoValidator.ValidateAsync( registerDto );
            if ( !validationResult.IsValid )
            {
                var errors = validationResult.Errors.Select( e => new
                {
                    Campo = e.PropertyName,
                    Mensagem = e.ErrorMessage
                } );
                return BadRequest( new
                {
                    Sucesso = false,
                    Erros = errors
                } );
            }

            var result = await _loginUserService.RegisterAsync( registerDto );
            if ( !result )
            {
                return BadRequest( "Não foi possivel realizar o cadastro" );
            }

            return CreatedAtAction( nameof( Register ), new { registerDto.email },
                "Usuario cadastrado com sucesso" );
        } catch ( Exception e )
        {
            _logger.LogError( e, "Erro ao realizar cadastro" );
            throw;
        }
    }

    /// <summary>
    /// Autentica um usuário e retorna um “token” JWT.
    /// </summary>
    /// <param name="loginUserDto">Credenciais do usuário para login.</param>
    /// <returns>Retorna um “token” de acesso se a autenticação for bem-sucedida.</returns>
    [HttpPost( "login" )]
    [ProducesResponseType( typeof( string ), StatusCodes.Status200OK )]
    [ProducesResponseType( StatusCodes.Status401Unauthorized )]
    public async Task<IActionResult> Login( [FromBody] LoginUserDto loginUserDto )
    {
        var token = await _loginUserService.LoginAsync( loginUserDto );
        if ( token == null )
        {
            return Unauthorized( "Credenciais invalidas" );
        }

        return Ok( new { token } );
    }
}