using Finrex_App.DTOS;
using Finrex_App.Services;
using Finrex_App.Services.Interface;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route( "api/[controller]" )]
public class AuthController : ControllerBase
{
    private readonly IAuthServices _authService;
    private readonly CriarUsuarioTeste _criarUsuarioTeste;

    public AuthController( IAuthServices authService, CriarUsuarioTeste criarUsuarioTeste )
    {
        _authService = authService;
        _criarUsuarioTeste = criarUsuarioTeste;
    }

    [HttpPost( "login" )]
    public async Task<IActionResult> Login( LoginResponseDto loginDto )
    {
        var result = await _authService.LoginAsync( loginDto );
        if ( result == null )
        {
            return Unauthorized();
        }

        return Ok( result );
    }

    [HttpPost( "register" )]
    public async Task<IActionResult> Register( LoginDto loginDto )
    {
        try
        {
            var userCreated = await _criarUsuarioTeste.CriarUsuarioAsync( loginDto );
            if ( !userCreated )
            {
                return Conflict( "Usuário já existe." );
            }

            return Created( "", "Usuário criado com sucesso" );
        } catch ( Exception e )
        {
            Console.WriteLine( $"Erro: {e.Message}" );
            return StatusCode( 500, "Erro ao criar usuário" );
        }
    }
}