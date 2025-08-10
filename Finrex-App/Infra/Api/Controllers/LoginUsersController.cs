using Finrex_App.Application.DTOs;
using Finrex_App.Application.Services.Interface;
using Finrex_App.Application.Validators;
using Finrex_App.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    /// <response code="201">Usuário cadastrado com sucesso.</response>
    /// <response code="400">Dados inválidos ou erro ao processar o cadastro.</response>
    /// <response code="500">Erro interno do servidor.</response>
    [HttpPost( "register" )]
    [ProducesResponseType( StatusCodes.Status201Created )]
    [ProducesResponseType( StatusCodes.Status400BadRequest )]
    [ProducesResponseType( StatusCodes.Status500InternalServerError )]
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

            return CreatedAtAction( nameof( Register ), new { email = registerDto.email },
                "Usuario cadastrado com sucesso" );
        } catch ( Exception e )
        {
            _logger.LogError( e, "Erro ao realizar cadastro" );
            throw;
        }
    }

    /// <summary>
    /// Autentica um usuário e retorna um token JWT.
    /// </summary>
    /// <param name="loginUserDto">Credenciais do usuário para login.</param>
    /// <returns>Retorna um token de acesso se a autenticação for bem-sucedida.</returns>
    /// <response code="200">Login bem-sucedido e token retornado.</response>
    /// <response code="401">Credenciais inválidas.</response>
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

    /// <summary>
    /// Verifica se um usuário existe pelo email informado.
    /// </summary>
    /// <param name="email">Email do usuário para verificação.</param>
    /// <returns>Retorna true se o usuário existe, false caso contrário.</returns>
    /// <response code="200">Verificação realizada com sucesso.</response>
    /// <response code="400">Email inválido ou não informado.</response>
    /// <response code="401">Acesso não autorizado.</response>
    [Authorize]
    [HttpGet( "check-user" )]
    [ProducesResponseType( StatusCodes.Status200OK )]
    [ProducesResponseType( StatusCodes.Status400BadRequest )]
    [ProducesResponseType( StatusCodes.Status401Unauthorized )]
    public async Task<ActionResult<object>> CheckUser( string email )
    {
        if ( string.IsNullOrEmpty( email ) )
        {
            return BadRequest( new { message = "Email deve ser informado." } );
        }

        var exists = await _loginUserService.UserExistsAsync( email );

        return Ok( new { exists } );
    }
}