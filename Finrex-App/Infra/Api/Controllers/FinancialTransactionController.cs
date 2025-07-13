using System.Security.Claims;
using Finrex_App.Application.DTOs;
using Finrex_App.Application.Services.Interface;
using Finrex_App.Application.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finrex_App.Infra.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class FinancialTransactionController : ControllerBase
{
    private readonly IFinancialTransactionService _financialTransactionService;
    private readonly ILoginUserServices _loginUserServices;
    private readonly ILogger<FinancialTransactionController> _logger;
    private readonly MIncomeDTOValidator _dtoValidator;

    public FinancialTransactionController(
        IFinancialTransactionService financialTransactionService, ILoginUserServices loginUserServices, ILogger<FinancialTransactionController> logger, MIncomeDTOValidator dtoValidator )
    {
        _financialTransactionService = financialTransactionService;
        _loginUserServices = loginUserServices;
        _logger = logger;
        _dtoValidator = dtoValidator;
    }
    
    [HttpPost("income")]
    public async Task<IActionResult> RegisterMIncomeAsync( MIncomeDto mIncomeDto )
    {
        try
        {
            var usuarioId = User.FindFirst( ClaimTypes.NameIdentifier )?.Value;
            Console.WriteLine($"Claims: {string.Join(", ", User.Claims.Select(c => $"{c.Type}: {c.Value}"))}");
            if ( string.IsNullOrEmpty( usuarioId ) )
            {
                return Unauthorized("Id do usuário não encontrado no token");
            }
            var validationResult = await _dtoValidator.ValidateAsync( mIncomeDto );
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
        
            var result = await _financialTransactionService.RegisterMIncomeAsync( mIncomeDto, Convert.ToInt32( usuarioId ) );
            if ( !result )
            {
                return BadRequest("Não foi possivel realizar o cadastro");
            }
            
            return Ok(new
            {
                Sucesso = true,
                Mensagem = "Dados cadastrados com sucesso",
                Dados = mIncomeDto
            });
        } catch ( Exception e )
        {
            _logger.LogError( e.Message, "Erro ao realizar cadastro" );
            throw;
        }
         
    }
    
}