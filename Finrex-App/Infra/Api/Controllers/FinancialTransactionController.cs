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
    [HttpPost( "spendings" )]
    public async Task<IActionResult> RegisterMSpendingAsync( MSpendingDtO mSpendingDto )
    {
        if ( mSpendingDto == null )
        {
            return BadRequest( new
                { Sucesso = false, Mensagem = "O corpo da requisição está vazio ou em um formato inválido." } );
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

    [HttpGet( "summary" )]
    public async Task<ActionResult<SummaryResponse>> GetSummary(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null
    )
    {
        try
        {
            var incomeQuery = _dbContext.MIncome.AsQueryable();
            var spendingQuery = _dbContext.MSpending.AsQueryable();

            if ( startDate.HasValue )
            {
                var startDateOnly = DateOnly.FromDateTime( startDate.Value );
                incomeQuery = incomeQuery.Where( i => i.Date >= startDateOnly );
                spendingQuery = spendingQuery.Where( s => s.Date >= startDateOnly );
            }

            if ( endDate.HasValue )
            {
                var endOfMonth = new DateOnly( endDate.Value.Year, endDate.Value.Month, 1 ).AddMonths( 1 );
                incomeQuery = incomeQuery.Where( i => i.Date < endOfMonth );
                spendingQuery = spendingQuery.Where( s => s.Date < endOfMonth );
            }

            var period = startDate.HasValue && endDate.HasValue
                ? $"{startDate.Value:yyyy-MM} à {endDate.Value:yyyy-MM}"
                : "Todo periodo";

            var income = await incomeQuery
                .GroupBy( i => 1 )
                .Select( g => new IncomeSummaryDto
                {
                    Period = period,
                    MainIncome = g.Sum( i => i.MainIncome ),
                    Freelance = g.Sum( i => i.Freelance ),
                    Benefits = g.Sum( i => i.Benefits ),
                    BussinesProfit = g.Sum( i => i.Benefits ),
                    Other = g.Sum( i => i.Other )
                } ).ToListAsync();

            var spending = await spendingQuery
                .GroupBy( s => 1 )
                .Select( g => new SpendingSummaryDto
                {
                    Period = period,
                    Transportation = g.Sum( s => s.Transportation ),
                    Groceries = g.Sum( s => s.Groceries ),
                    Entertainment = g.Sum( s => s.Entertainment ),
                    Rent = g.Sum( s => s.Rent ),
                    Utilities = g.Sum( s => s.Utilities )
                } ).ToListAsync();

            return Ok( new SummaryResponse
            {
                Income = income,
                Spending = spending
            } );
        } catch ( Exception )
        {
            return StatusCode( StatusCodes.Status500InternalServerError,
                "Ocorreu um erro ao processar sua solicitação."
            );
        }
    }
}