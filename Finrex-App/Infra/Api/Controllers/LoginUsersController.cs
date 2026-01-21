using Finrex_App.Application.DTOs;
using Finrex_App.Application.Services.Interface;
using Finrex_App.Application.Validators;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using FluentValidation;

namespace Finrex_App.Infra.Api.Controllers;

/// <inheritdoc />
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/login-users")]
public class LoginUsersController(
    ILoginUserServices loginUserService,
    ILogger<LoginUsersController> logger,
    RegisterDTOValidator dtoValidator,
    IAntiforgery antiforgery,
    IConfiguration configuration)
    : ControllerBase
{
    private void SetAuthCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddHours(8)
        };
        Response.Cookies.Append("finrex.auth", token, cookieOptions);
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
    {
        await dtoValidator.ValidateAsync(registerDto);

        var result = await loginUserService.RegisterAsync(registerDto);
        if (!result)
        {
            var response = ApiResponse<string>.CreateFailure("Não foi possivel realizar o cadastro");
            return BadRequest(response);
        }

        var successResponse = ApiResponse<object>.CreateSuccess(new { registerDto.email },
            "Usuario cadastrado com sucesso");
        return Ok(successResponse);
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginUserDto loginUserDto)
    {
        var token = await loginUserService.LoginAsync(loginUserDto);
        if (token == null)
        {
            var response = ApiResponse<string>.CreateFailure("Credenciais invalidas");
            return Unauthorized(response);
        }

        SetAuthCookie(token);
        var successResponse =
            ApiResponse<object>.CreateSuccess(new { token = token });
        return Ok(successResponse);
    }

    [HttpGet("google-login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GoogleLogin()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(GoogleSignInCallback))
        };

        // The "prompt" parameter to force account selection even when one account is available.
        properties.SetParameter("prompt", "select_account");

        return Challenge(properties, "Google");
    }

    [HttpGet("google-signin-callback")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GoogleSignInCallback()
    {
        var result = await HttpContext.AuthenticateAsync("Cookies");

        if (!result.Succeeded) return Unauthorized("Falha na autenticação com o Google. Verifique os logs.");

        var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
        var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

        logger.LogInformation("Email obtido: {Email}, Nome: {Name}", email, name);

        if (string.IsNullOrEmpty(email)) return BadRequest("Não foi possível obter o e-mail do Google.");


        var token = await loginUserService.HandleGoogleLoginAsync(email, name);
        if (token == null) return Unauthorized("Não foi possível processar o login com o Google.");

        logger.LogInformation("Token gerado com sucesso para o email {Email}", email);
        SetAuthCookie(token);
        var frontendBase = configuration["FrontendBaseUrl"];
        if (string.IsNullOrWhiteSpace(frontendBase)) frontendBase = "http://localhost:3000";

        var frontendUrl = $"{frontendBase}/insights?token={token}";
        return Redirect(frontendUrl);
    }

    [HttpDelete("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Logout()
    {
        var providerClaim = User.FindFirst("auth_provider")?.Value;
        if (providerClaim == "google") await HttpContext.SignOutAsync("Google");
        Response.Cookies.Delete("finrex.auth");
        var successResponse = ApiResponse<string>.CreateSuccess("Logout realizado com sucesso");
        return Ok(successResponse);
    }

    [HttpGet("get-csrf-token")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult GetCsrfToken()
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        var requestToken = tokens.RequestToken;
        return Ok(new { csrfToken = requestToken });
    }
}