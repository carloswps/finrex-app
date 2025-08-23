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
    private readonly MIncomeDTOValidator _dtoMIValidator;
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
        _dtoMIValidator = dtoValidatorMi;
    }

    /// <summary>
    /// Registra uma nova entrada de renda mensal para o usuário autenticado.
    /// </summary>
    /// <param name="mIncomeDto">Dados da renda mensal a ser registrada.</param>
    /// <returns>Retorna o status do cadastro da renda.</returns>
    /// <response code="200">Renda cadastrada com sucesso.</response>
    /// <response code="400">Dados inválidos ou erro ao processar o cadastro.</response>
    /// <response code="401">Usuário não autorizado.</response>
    [HttpPost( "income" )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RegisterMIncomeAsync( MIncomeDto mIncomeDto )
    {
        try
        {
            var usuarioId = User.FindFirst( ClaimTypes.NameIdentifier )?.Value;
            Console.WriteLine( $"Claims: {string.Join( ", ", User.Claims.Select( c => $"{c.Type}: {c.Value}" ) )}" );
            if ( string.IsNullOrEmpty( usuarioId ) )
            {
                return Unauthorized( "Id do usuário não encontrado no token" );
            }

            var validationResult = await _dtoMIValidator.ValidateAsync( mIncomeDto );
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
    /// Registra uma nova despesa mensal para o usuário autenticado.
    /// </summary>
    /// <param name="mSpendingDto">Dados da despesa mensal a ser registrada.</param>
    /// <returns>Retorna o status do cadastro da despesa.</returns>
    /// <response code="200">Despesa cadastrada com sucesso.</response>
    /// <response code="400">Dados inválidos ou erro ao processar o cadastro.</response>
    /// <response code="401">Usuário não autorizado.</response>
    [HttpPost( "spending" )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RegisterMSpendingAsync( MSpendingDtO mSpendingDto )
    {
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