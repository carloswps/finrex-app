using System.Security.Claims;
using Finrex_App.Application.DTOs;
using Finrex_App.Application.Services.Interface;
using Finrex_App.Application.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finrex_App.Infra.Api.Controllers;

[ApiController]
[ApiVersion( "1.0" )]
[Route( "api/v{version:apiVersion}/[controller]" )]
[Authorize]
public class FinancialTransactionController : ControllerBase
{
    private readonly MIncomeDTOValidator _dtoMiValidator;
    private readonly MSpendingDTOValidator _dtoMsValidator;
    private readonly IFinancialTransactionService _financialTransactionService;
    private readonly ILogger<FinancialTransactionController> _logger;

    public FinancialTransactionController(
        IFinancialTransactionService financialTransactionService,
        ILogger<FinancialTransactionController> logger, MIncomeDTOValidator dtoValidatorMi,
        MSpendingDTOValidator dtoMsValidator )
    {
        _financialTransactionService = financialTransactionService;
        _logger = logger;
        _dtoMsValidator = dtoMsValidator;
        _dtoMiValidator = dtoValidatorMi;
    }

    /// <summary>
    /// Registra uma nova entrada de renda mensal para o usuário autenticado.
    /// </summary>
    /// <param name="mIncomeDto">Dados da renda mensal a ser registrada.</param>
    /// <returns>Retorna o status do cadastro da renda.</returns>
    [HttpPost( "income" )]
    public async Task<IActionResult> RegisterMIncomeAsync( MIncomeDto mIncomeDto )
    {
        if ( mIncomeDto == null )
        {
            return BadRequest( new { Sucesso = false, Mensagem = "O corpo da requisição está vazio ou em um formato inválido." } );
        }
        try
        {
            var usuarioId = User.FindFirst( ClaimTypes.NameIdentifier )?.Value;
            if ( string.IsNullOrEmpty( usuarioId ) )
            {
                return Unauthorized( "Id do usuário não encontrado no token" );
            }

            var validationResult = await _dtoMiValidator.ValidateAsync( mIncomeDto );
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

            var result = await _financialTransactionService.RegisterMIncomeAsync( mIncomeDto,
                Convert.ToInt32( usuarioId ) );
            if ( !result )
            {
                return BadRequest( "Não foi possivel realizar o cadastro" );
            }

            return Ok( new
            {
                Sucesso = true,
                Mensagem = "Dados cadastrados com sucesso",
                Dados = mIncomeDto
            } );
        } catch ( Exception e )
        {
            _logger.LogError( e.Message, "Erro ao realizar cadastro" );
            throw;
        }
    }

    /// <summary>
    /// Registrar uma nova despesa mensal para o usuário autenticado.
    /// </summary>
    /// <param name="mSpendingDto">Dados da despesa mensal a ser registrada.</param>
    [HttpPost( "spending" )]
    public async Task<IActionResult> RegisterMSpendingAsync( MSpendingDtO mSpendingDto )
    {
        if ( mSpendingDto == null )
        {
            return BadRequest( new { Sucesso = false, Mensagem = "O corpo da requisição está vazio ou em um formato inválido." } );
        }
        var userId = User.FindFirst( ClaimTypes.NameIdentifier )?.Value;
        if ( string.IsNullOrEmpty( userId ) )
        {
            return Unauthorized( "Id do usuario não encontrado no token" );
        }

        var validationResult = await _dtoMsValidator.ValidateAsync( mSpendingDto );
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

        var result = await _financialTransactionService.RegisterMSpendingAsync( mSpendingDto,
            Convert.ToInt32( userId ) );
        if ( !result )
        {
            return BadRequest( "Não foi possivel realizar o cadastro" );
        }

        return Ok( new
        {
            Sucesso = true,
            Mensagem = "Dados cadastrados com sucesso",
            Dados = mSpendingDto
        } );
    }
}