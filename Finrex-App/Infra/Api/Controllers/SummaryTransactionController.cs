using System.Runtime.InteropServices.JavaScript;
using Finrex_App.Application.DTOs;
using Finrex_App.Infra.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Finrex_App.Infra.Api.Controllers;

[ApiController]
[ApiVersion( "1.0" )]
[Route( "api/v{version:apiVersion}/[controller]" )]
[Authorize]
public class SummaryTransactionController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public SummaryTransactionController( AppDbContext dbContext )
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Obtém um resumo das transações financeiras (receitas e despesas) num período especificado.
    /// </summary>
    /// <param name="startDate">Data de início do período do resumo.</param>
    /// <param name="endDate">Data de fim do período do resumo.</param>
    /// <returns>Retorna um resumo das transações financeiras.</returns>
    /// <response code="200">Resumo retornado com sucesso.</response>
    /// <response code="500">Erro interno do servidor.</response>
    [HttpGet( "summary" )]
    [ProducesResponseType( StatusCodes.Status200OK )]
    [ProducesResponseType( StatusCodes.Status500InternalServerError )]
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
                    BussinesProfit = g.Sum( i => i.BussinesProfit ),
                    Other = g.Sum( i => i.Other )
                } )
                .ToListAsync();

            var spending = await spendingQuery
                .GroupBy( s => 1 )
                .Select( g => new SpendingSummaryDto
                {
                    Period = period,
                    Transportation = g.Sum( s => s.Transportation ),
                    Entertainment = g.Sum( s => s.Entertainment ),
                    Rent = g.Sum( s => s.Rent ),
                    Groceries = g.Sum( s => s.Groceries ),
                    Utilities = g.Sum( s => s.Utilities )
                } )
                .ToListAsync();

            return Ok( new SummaryResponse
            {
                Income = income,
                Spending = spending
            } );
        } catch ( Exception )
        {
            return StatusCode( StatusCodes.Status500InternalServerError,
                "Ocorreu um erro ao processar sua solicitação." );
        }
    }
}