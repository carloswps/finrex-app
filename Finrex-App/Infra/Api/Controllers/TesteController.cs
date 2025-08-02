using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controlador para testar o tratamento de diferentes tipos de erros na API.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TesteErrosController : ControllerBase
{
    /// <summary>
    /// Testa o tratamento de erro para um registro não encontrado (404 Not Found).
    /// </summary>
    [HttpGet("erro-nao-encontrado")]
    public IActionResult TesteNotFound()
    {
        throw new KeyNotFoundException("Registro não encontrado");
    }

    /// <summary>
    /// Testa o tratamento de erro para uma requisição inválida (400 Bad Request).
    /// </summary>
    [HttpGet("erro-bad-request")]
    public IActionResult TesteBadRequest()
    {
        throw new ArgumentException("Dados inválidos");
    }

    /// <summary>
    /// Testa o tratamento de erro para acesso não autorizado (401 Unauthorized).
    /// </summary>
    [HttpGet("erro-nao-autorizado")]
    public IActionResult TesteUnauthorized()
    {
        throw new UnauthorizedAccessException("Acesso não autorizado");
    }

    /// <summary>
    /// Testa o tratamento de erro interno do servidor (500 Internal Server Error).
    /// </summary>
    [HttpGet("erro-interno")]
    public IActionResult TesteErroInterno()
    {
        throw new Exception("Erro interno do servidor");
    }
}