using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class TesteErrosController : ControllerBase
{
    [HttpGet("erro-nao-encontrado")]
    public IActionResult TesteNotFound()
    {
        throw new KeyNotFoundException("Registro não encontrado");
    }

    [HttpGet("erro-bad-request")]
    public IActionResult TesteBadRequest()
    {
        throw new ArgumentException("Dados inválidos");
    }

    [HttpGet("erro-nao-autorizado")]
    public IActionResult TesteUnauthorized()
    {
        throw new UnauthorizedAccessException("Acesso não autorizado");
    }

    [HttpGet("erro-interno")]
    public IActionResult TesteErroInterno()
    {
        throw new Exception("Erro interno do servidor");
    }
}