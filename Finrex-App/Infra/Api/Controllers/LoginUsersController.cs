using Finrex_App.Application.DTOs;
using Finrex_App.Application.Services.Interface;
using Finrex_App.Application.Validators;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

    [HttpGet( "google-login" )]
    public IActionResult GoogleLogin()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action( nameof( GoogleSignInCallback ) )
        };

        // The "prompt" parameter to force account selection even when one account is available.
        //properties.SetParameter( "prompt", "select_account" );

        return Challenge( properties, "Google" );
    }

    [HttpGet( "google-signin-callback" )] [ApiExplorerSettings( IgnoreApi = true )]
    public async Task<IActionResult> GoogleSignInCallback()
    {
        var result = await HttpContext.AuthenticateAsync( "Cookies" );

        if ( !result.Succeeded )
        {
            return Unauthorized( "Falha na autenticação com o Google. Verifique os logs." );
        }

        var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
        var email = claims?.FirstOrDefault( c => c.Type == ClaimTypes.Email )?.Value;
        var name = claims?.FirstOrDefault( c => c.Type == ClaimTypes.Name )?.Value;

        _logger.LogInformation( "Email obtido: {Email}, Nome: {Name}", email, name );

        if ( string.IsNullOrEmpty( email ) )
        {
            return BadRequest( "Não foi possível obter o e-mail do Google." );
        }

        try
        {
            var token = await _loginUserService.HandleGoogleLoginAsync( email, name );
            if ( token == null )
            {
                return Unauthorized( "Não foi possível processar o login com o Google." );
            }

            _logger.LogInformation( "Token gerado com sucesso para o email {Email}", email );
            return Ok( new { token } );
        } catch ( Exception ex )
        {
            _logger.LogError( ex, "Erro ao chamar HandleGoogleLoginAsync para o email {Email}", email );
            throw;
        }
    }

    [HttpPost( "logout" )]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync( "Cookies" );
        return Ok( "Logout realizado com sucesso" );
    }
}