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
    /// Obtém um resumo das transações financeiras (receitas e despesas) dentro de um período especificado.
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
                var endDateOnly = DateOnly.FromDateTime( endDate.Value );
                incomeQuery = incomeQuery.Where( i => i.Date <= endDateOnly );
                spendingQuery = spendingQuery.Where( s => s.Date <= endDateOnly );
            }

            var income = await incomeQuery
                .GroupBy( x => new { x.Date.Year, x.Date.Month } )
                .Select( x => new IncomeSummaryDto
                {
                    Period = $"{x.Key.Year}-{x.Key.Month:D2}",
                    MainIncome = x.Sum( i => i.MainIncome ),
                    Freelance = x.Sum( i => i.Freelance ),
                    Benefits = x.Sum( i => i.Benefits ),
                    BussinesProfit = x.Sum( i => i.BussinesProfit ),
                    Other = x.Sum( i => i.Other )
                } )
                .ToListAsync();

            var spending = await spendingQuery
                .GroupBy( x => new { x.Date.Year, x.Date.Month } )
                .Select( x => new SpendingSummaryDto
                {
                    Period = $"{x.Key.Year}-{x.Key.Month:D2}",
                    Transportation = x.Sum( s => s.Transportation ),
                    Entertainment = x.Sum( s => s.Entertainment ),
                    Rent = x.Sum( s => s.Rent ),
                    Groceries = x.Sum( s => s.Groceries ),
                    Utilities = x.Sum( s => s.Utilities )
                } )
                .ToListAsync();

            return Ok( new SummaryResponse
            {
                Income = income,
                Spending = spending
            } );
        } catch ( Exception e )
        {
            return StatusCode( StatusCodes.Status500InternalServerError,
                "Ocorreu um erro ao processar sua solicitação." );
        }
    }
}