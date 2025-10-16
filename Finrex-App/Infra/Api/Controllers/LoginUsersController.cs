using Finrex_App.Application.DTOs;
using Finrex_App.Application.Services.Interface;
using Finrex_App.Application.Validators;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Finrex_App.Infra.Api.Controllers;

[ApiController]
[ApiVersion( "1.0" )]
[Route( "api/v{version:apiVersion}/login-users" )]
public class LoginUsersController : ControllerBase
{
    private readonly ILoginUserServices _loginUserService;
    private readonly ILogger<LoginUsersController> _logger;
    private readonly RegisterDTOValidator _dtoValidator;
    private readonly IAntiforgery _antiforgery;
    private readonly IConfiguration _configuration;

    public LoginUsersController(
        ILoginUserServices loginUserService, ILogger<LoginUsersController> logger, RegisterDTOValidator dtoValidator,
        IAntiforgery antiforgery, IConfiguration configuration )
    {
        _loginUserService = loginUserService;
        _logger = logger;
        _dtoValidator = dtoValidator;
        _antiforgery = antiforgery;
        _configuration = configuration;
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

        Response.Cookies.Append( "finrex.auth", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddHours( 8 )
        } );

        return Ok( new { message = "Login realizado com sucesso" } );
    }

    [HttpGet( "google-login" )]
    public IActionResult GoogleLogin()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action( nameof( GoogleSignInCallback ) )
        };

        // The "prompt" parameter to force account selection even when one account is available.
        properties.SetParameter( "prompt", "select_account" );

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

            Response.Cookies.Append( "finrex.auth", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours( 8 )
            } );
            var frontendUrl = $"{_configuration[ "FrontendBaseUrl" ]}/insights";
            return Redirect( frontendUrl );
        } catch ( Exception ex )
        {
            _logger.LogError( ex, "Erro ao chamar HandleGoogleLoginAsync para o email {Email}", email );
            var errorUrl = $"{_configuration[ "FrontendBaseUrl" ]}/login?error=unexpected-error";
            return Redirect( errorUrl );
        }
    }

    [HttpDelete( "logout" )]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var providerClaim = User.FindFirst( "auth_provider" );

        if ( providerClaim is null )
        {
            return Unauthorized( "Não foi possivel realizar o logout" );
        }

        switch ( providerClaim.Value )
        {
            case "google":
                await HttpContext.SignOutAsync( "Cookies" );
                break;
            case "password":
                break;
        }

        return NoContent();
    }

    [HttpGet( "get-csrf-token" )] [ApiExplorerSettings( IgnoreApi = true )]
    public IActionResult GetCsrfToken()
    {
        var tokens = _antiforgery.GetAndStoreTokens( HttpContext );
        var requestToken = tokens.RequestToken;
        return Ok( new { csrfToken = requestToken } );
    }
}