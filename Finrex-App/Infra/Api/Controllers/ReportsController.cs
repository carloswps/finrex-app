using System.Security.Claims;
using Finrex_App.Application.DTOs;
using Finrex_App.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finrex_App.Infra.Api.Controllers;

[ApiController]
[ApiVersion( "1.0" )]
[Route( "api/v{version:apiVersion}/reports" )]
[Authorize]
public class ReportsController : Controller
{
    private readonly IFinancialTransactionService _financialTransactionService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        IFinancialTransactionService financialTransactionService, ILogger<ReportsController> logger )
    {
        _financialTransactionService = financialTransactionService;
        _logger = logger;
    }

    private int? GetCurrentUserId()
    {
        var userIdString = User.FindFirst( "userId" )?.Value
                           ?? User.FindFirst( ClaimTypes.NameIdentifier )?.Value;
        if ( int.TryParse( userIdString, out var userId ) )
        {
            return userId;
        }

        return null;
    }

    private (DateOnly FirstMonth, DateOnly LastMonth) ParseDateRange(
        string? firstMonthStr, string? lastMonthStr
    )
    {
        var today = DateTime.Today;
        var currentMonth = new DateOnly( today.Year, today.Month, 1 );

        var lastMonth = !string.IsNullOrEmpty( lastMonthStr ) &&
                        DateOnly.TryParse( lastMonthStr + "-01", out var parsedLast )
            ? parsedLast
            : currentMonth;

        var firstMonth = !string.IsNullOrEmpty( firstMonthStr ) &&
                         DateOnly.TryParse( firstMonthStr + "-01", out var parsedFirst )
            ? parsedFirst
            : lastMonth.AddMonths( -1 );

        return ( firstMonth, lastMonth );
    }

    [HttpGet( "summary" )]
    [ProducesResponseType(typeof(SummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SummaryResponse>> GetSummary(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null )
    {
        try
        {
            var userId = GetCurrentUserId();
            if ( userId == null )
            {
                return Unauthorized( "O usuário não possui as credências necessárias" );
            }

            var summary = await _financialTransactionService.GetSummaryAsync( startDate, endDate, userId.Value );

            return Ok( summary );
        } catch ( Exception e )
        {
            return StatusCode( StatusCodes.Status500InternalServerError,
                "Ocorreu um erro ao processar sua solicitação."
            );
        }
    }

    [HttpGet( "savings-growth" )]
    [ProducesResponseType(typeof(SavingsGrowthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SavingsGrowthResult>> GetSavingsGrowth(
        [FromQuery] string? firstMonth = null,
        [FromQuery] string? lastMonth = null
    )
    {
        try
        {
            var userId = GetCurrentUserId();
            if ( userId == null )
            {
                return Unauthorized( "O usuário não possui as credências necessárias" );
            }

            var (fMonth, lMonth) = ParseDateRange( firstMonth, lastMonth );
            var result = await _financialTransactionService.GetSavingsGrowthAsync( userId.Value, fMonth, lMonth );

            return Ok( result );
        } catch ( Exception e )
        {
            _logger.LogError( e, "Erro ao buscar o crescimento de poupança." );
            return StatusCode( StatusCodes.Status500InternalServerError,
                "Ocorreu um erro interno ao processar a solicitação." );
        }
    }

    [HttpGet( "net-profit-growth" )]
    [ProducesResponseType(typeof(NetProfitResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NetProfitResult>> GetNetProfit(
        [FromQuery] string? firstMonth = null,
        [FromQuery] string? lastMonth = null
    )
    {
        try
        {
            var userId = GetCurrentUserId();
            if ( userId == null )
            {
                return Unauthorized( "O usuário não possui as credências necessárias" );
            }

            var (fMonth, lMonth) = ParseDateRange( firstMonth, lastMonth );

            var result = await _financialTransactionService.GetNetProfitGrowthAsync( userId.Value, fMonth, lMonth );

            return Ok( result );
        } catch ( Exception e )
        {
            _logger.LogError( e, "Erro ao buscar o Lucro Líquido." );
            return StatusCode( StatusCodes.Status500InternalServerError,
                "Ocorreu um erro interno ao processar a solicitação." );
        }
    }

    [HttpGet( "spending-comparison" )]
    [ProducesResponseType(typeof(SpendingComparison), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SpendingComparison>> GetSpendingComparison(
        [FromQuery] string? firstMonth = null,
        [FromQuery] string? lastMonth = null
    )
    {
        try
        {
            var userId = GetCurrentUserId();
            if ( userId == null )
            {
                return Unauthorized( "O usuário não possui as credências necessárias" );
            }

            var (fMonth, lMonth) = ParseDateRange( firstMonth, lastMonth );

            var result = await _financialTransactionService.GetSpendingComparisonAsync( userId.Value, fMonth, lMonth );
            return Ok( result );
        } catch ( Exception e )
        {
            _logger.LogError( e, "Erro ao buscar os dados Líquido." );
            return StatusCode( StatusCodes.Status500InternalServerError,
                "Ocorreu um erro interno ao processar a solicitação." );
        }
    }

    [HttpGet( "month-present" )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentMonthSpendings(
        [FromQuery] string? firstMonth = null,
        [FromQuery] string? lastMonth = null
    )
    {
        try
        {
            var userId = GetCurrentUserId();
            if ( userId == null )
            {
                return Unauthorized( "O usuário não possui as credências necessárias" );
            }

            var (fMonth, lMonth) = ParseDateRange( firstMonth, lastMonth );

            var result = await _financialTransactionService.GetCurrentMonthSpendingsAsync(
                userId.Value, fMonth, lMonth );

            return Ok( result );
        } catch ( Exception e )
        {
            _logger.LogError( e, "Erro ao buscar os dados Líquido." );
            return StatusCode( StatusCodes.Status500InternalServerError,
                "Ocorreu um erro interno ao processar a solicitação." );
        }
    }

    [HttpGet( "top-earnings" )]
    [ProducesResponseType(typeof(TopEarningMonth), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<TopEarningMonth>>> GetTopEarningMonths()
    {
        try
        {
            var userId = GetCurrentUserId();
            if ( userId == null )
            {
                return Unauthorized( "O usuário não possui as credências necessárias" );
            }

            var result = await _financialTransactionService.GetTopEarningMonthAsync( userId.Value );

            if ( !result.Any() )
            {
                return NotFound( "Nenhuma receita encontrada para o usuário." );
            }

            return Ok( result );
        } catch ( Exception e )
        {
            _logger.LogError( e, "Erro ao buscar os dados Líquido." );
            return StatusCode( StatusCodes.Status500InternalServerError,
                "Ocorreu um erro interno ao processar a solicitação." );
        }
    }

    [HttpGet("top-savings")]
    [ProducesResponseType(typeof(TopSavingsMonth), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<TopSavingsMonth>>> GetTopSavingsMonths()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized("O usuário não possui as credências necessárias");
            }

            var result = await _financialTransactionService.GetTopSavingsMonthAsync(userId.Value);

            if (!result.Any())
            {
                return NotFound("Nenhuma economia registrada entre meses.");
            }

            return Ok(result);

        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao buscar os dados Líquido.");
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Ocorreu um erro interno ao processar a solicitação.");
        }
    }

    [HttpGet("current-month-spenging")]
    [ProducesResponseType(typeof(SpendingSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SpendingSummaryDto>> GetCurrentMonthSpendingsReport()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized("O usuário não possui as credências necessárias");
            }

            var result = await _financialTransactionService.GetCurrentMonthSpendingSummaryAsync(userId.Value);

            if (result == null)
            {
                return NotFound("Nenhum gasto encontrado para o mês atual.");
            }

            return Ok(result);

        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao buscar os gastos do mês atual.");
            return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro interno ao processar a solicitação.");
        }
    }
}