using System.Security.Claims;
using Finrex_App.Application.DTOs;
using Finrex_App.Application.Services.Interface;
using Finrex_App.Application.Validators;
using Finrex_App.Infra.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Finrex_App.Infra.Api.Controllers;

[ApiController]
[ApiVersion( "1.0" )]
[Route( "api/v{version:apiVersion}/financial-transactions" )]
[Authorize]
public class FinancialTransactionController : ControllerBase
{
    private string? GetUserId()
    {
        // Tries common claim names emitted by different identity providers
        return User.FindFirst( "userId" )?.Value
               ?? User.FindFirst( ClaimTypes.NameIdentifier )?.Value
               ?? User.FindFirst( "sub" )?.Value
               ?? User.FindFirst( "nameid" )?.Value;
    }

    private readonly MIncomeDTOValidator _dtoMiValidator;
    private readonly MSpendingDTOValidator _dtoMsValidator;
    private readonly IFinancialTransactionService _financialTransactionService;
    private readonly ILogger<FinancialTransactionController> _logger;
    private readonly AppDbContext _dbContext;

    public FinancialTransactionController(
        IFinancialTransactionService financialTransactionService,
        ILogger<FinancialTransactionController> logger, MIncomeDTOValidator dtoValidatorMi,
        MSpendingDTOValidator dtoMsValidator, AppDbContext dbContext )
    {
        _financialTransactionService = financialTransactionService;
        _logger = logger;
        _dtoMsValidator = dtoMsValidator;
        _dbContext = dbContext;
        _dtoMiValidator = dtoValidatorMi;
    }

    /// <summary>
    /// Registra uma nova entrada de renda mensal para o usuário autenticado.
    /// </summary>
    /// <param name="mIncomeDto">Dados da renda mensal a ser registrada.</param>
    /// <returns>Retorna o status do cadastro da renda.</returns>
    [HttpPost( "incomes" )]
    public async Task<IActionResult> RegisterMIncomeAsync( MIncomeDto mIncomeDto )
    {
        if ( mIncomeDto == null )
        {
            return BadRequest( new
                { Sucesso = false, Mensagem = "O corpo da requisição está vazio ou em um formato inválido." } );
        }

        try
        {
            var usuarioId = GetUserId();
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
    [HttpPost( "spendings" )]
    public async Task<IActionResult> RegisterMSpendingAsync( MSpendingDtO mSpendingDto )
    {
        if ( mSpendingDto == null )
        {
            return BadRequest( new
                { Sucesso = false, Mensagem = "O corpo da requisição está vazio ou em um formato inválido." } );
        }

        var userId = GetUserId();
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

    [HttpGet( "summary" )]
    public async Task<ActionResult<SummaryResponse>> GetSummary(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null
    )
    {
        try
        {
            var userId = GetUserId();
            if ( string.IsNullOrEmpty( userId ) )
            {
                return Unauthorized( "O usuário não possui as credências necessárias" );
            }

            var summary = await _financialTransactionService.GetSummaryAsync( startDate, endDate,
                Convert.ToInt32( userId ) );

            return Ok( summary );
        } catch ( Exception )
        {
            return StatusCode( StatusCodes.Status500InternalServerError,
                "Ocorreu um erro ao processar sua solicitação."
            );
        }
    }

    [HttpGet( "month-present" )]
    public async Task<IActionResult> GetCurrentMonthSpendings(
        [FromQuery] string? firstMonth = null,
        [FromQuery] string? lastMonth = null
    )
    {
        try
        {
            var userIdString = GetUserId();
            if ( string.IsNullOrEmpty( userIdString ) || !int.TryParse( userIdString, out var userId ) )
            {
                return Unauthorized( "O usuário não possui as credências necessárias " );
            }


            var currentMonth = new DateOnly( DateTime.Today.Year, DateTime.Today.Month, 1 );

            DateOnly comparisonLastMonth;
            if ( !string.IsNullOrEmpty( lastMonth ) && DateOnly.TryParse( lastMonth + "-01", out var parsedLastMonth ) )
            {
                comparisonLastMonth = parsedLastMonth;
            } else
            {
                comparisonLastMonth = currentMonth;
            }

            DateOnly comparisonFirstMonth;
            if ( !string.IsNullOrEmpty( firstMonth ) &&
                 DateOnly.TryParse( firstMonth + "-01", out var parsedFirstMonth ) )
            {
                comparisonFirstMonth = parsedFirstMonth;
            } else
            {
                comparisonFirstMonth = comparisonLastMonth.AddMonths( -1 );
            }

            var result = await _financialTransactionService.GetCurrentMonthSpendingsAsync(
                userId,
                comparisonFirstMonth,
                comparisonLastMonth
            );

            return Ok( result );
        } catch ( Exception e )
        {
            _logger.LogError( e, "Erro ao buscar os gastos do mês atual" );
            return StatusCode( 500 );
        }
    }

    [HttpGet( "savings-growth" )]
    public async Task<ActionResult<SavingsGrowthResult>> GetSavingsGrowth(
        [FromQuery] string? firstMonth = null,
        [FromQuery] string? lastMonth = null
    )
    {
        try
        {
            var userIdString = GetUserId();
            if ( string.IsNullOrEmpty( userIdString ) || !int.TryParse( userIdString, out var userId ) )
            {
                return Unauthorized( "O usuário não possui as credenciais necessárias." );
            }

            var currentMonth = new DateOnly( DateTime.Today.Year, DateTime.Today.Month, 1 );

            var comparisonLastMonth = !string.IsNullOrEmpty( lastMonth ) &&
                                      DateOnly.TryParse( lastMonth + "-01", out var parsedLastMonth )
                ? parsedLastMonth
                : currentMonth;

            var comparisonFirstMonth = !string.IsNullOrEmpty( firstMonth ) &&
                                       DateOnly.TryParse( firstMonth + "-01", out var parsedFirstMonth )
                ? parsedFirstMonth
                : comparisonLastMonth.AddMonths( -1 );

            var result = await _financialTransactionService.GetSavingsGrowthAsync(
                userId,
                comparisonFirstMonth,
                comparisonLastMonth
            );

            return Ok( result );
        } catch ( Exception e )
        {
            _logger.LogError( e, "Erro ao buscar o crescimento de poupança." );
            return StatusCode( StatusCodes.Status500InternalServerError,
                "Ocorreu um erro interno ao processar a solicitação." );
        }
    }

    [HttpGet( "net-profit" )]
    public async Task<ActionResult<NetProfitResult>> GetNetProfit(
        [FromQuery] string? firstMonth = null,
        [FromQuery] string? lastMonth = null
    )
    {
        try
        {
            var userIdString = GetUserId();
            if ( string.IsNullOrEmpty( userIdString ) || !int.TryParse( userIdString, out var userId ) )
            {
                return Unauthorized( "O usuário não possui as credenciais necessárias." );
            }

            var currentMonth = new DateOnly( DateTime.Today.Year, DateTime.Today.Month, 1 );

            var comparisonLastMonth = !string.IsNullOrEmpty( firstMonth ) &&
                                      DateOnly.TryParse( firstMonth + "-01", out var parsedLastMonth )
                ? parsedLastMonth
                : currentMonth;

            var comparisonFirstMonth = !string.IsNullOrEmpty( firstMonth ) &&
                                       DateOnly.TryParse( firstMonth + "-01", out var parsedFirstMonth )
                ? parsedFirstMonth
                : comparisonLastMonth.AddMonths( -1 );

            var result = await _financialTransactionService.GetNetProfitGrowthAsync(
                userId,
                comparisonFirstMonth,
                comparisonLastMonth
            );

            return Ok( result );
        } catch ( Exception e )
        {
            _logger.LogError( e, "Erro ao buscar o Lucro Líquido." );
            return StatusCode( StatusCodes.Status500InternalServerError,
                "Ocorreu um erro interno ao processar a solicitação." );
        }
    }
}