using System.Security.Claims;
using Finrex_App.Application.DTOs;
using Finrex_App.Application.Services.Interface;
using Finrex_App.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finrex_App.Infra.Api.Controllers;

/// <inheritdoc />
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/reports")]
[Authorize]
public class ReportsController(
    IFinancialTransactionService financialTransactionService,
    ILogger<ReportsController> logger) : Controller
{
    private readonly IFinancialTransactionService _financialTransactionService = financialTransactionService;
    private readonly ILogger<ReportsController> _logger = logger;

    private (DateOnly FirstMonth, DateOnly LastMonth) ParseDateRange(
        string? firstMonthStr, string? lastMonthStr
    )
    {
        var today = DateTime.Today;
        var currentMonth = new DateOnly(today.Year, today.Month, 1);

        var lastMonth = !string.IsNullOrEmpty(lastMonthStr) &&
                        DateOnly.TryParse(lastMonthStr + "-01", out var parsedLast)
            ? parsedLast
            : currentMonth;

        var firstMonth = !string.IsNullOrEmpty(firstMonthStr) &&
                         DateOnly.TryParse(firstMonthStr + "-01", out var parsedFirst)
            ? parsedFirst
            : lastMonth.AddMonths(-1);

        return (firstMonth, lastMonth);
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(SummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SummaryResponse>> GetSummary(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            var response = ApiResponse<string>.CreateFailure("Credenciais invalidas");
            return Unauthorized(response);
        }

        var summary = await _financialTransactionService.GetSummaryAsync(startDate, endDate, userId.Value);

        var successResponse = ApiResponse<SummaryResponse>.CreateSuccess(summary);
        return Ok(successResponse);
    }

    [HttpGet("savings-growth")]
    [ProducesResponseType(typeof(SavingsGrowthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SavingsGrowthResult>> GetSavingsGrowth(
        [FromQuery] string? firstMonth = null,
        [FromQuery] string? lastMonth = null
    )
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized("O usuário não possui as credências necessárias");
        var (fMonth, lMonth) = ParseDateRange(firstMonth, lastMonth);
        var result = await _financialTransactionService.GetSavingsGrowthAsync(userId.Value, fMonth, lMonth);
        return Ok(result);
    }

    [HttpGet("net-profit-growth")]
    [ProducesResponseType(typeof(NetProfitResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NetProfitResult>> GetNetProfit(
        [FromQuery] string? firstMonth = null,
        [FromQuery] string? lastMonth = null
    )
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized("O usuário não possui as credências necessárias");
        var (fMonth, lMonth) = ParseDateRange(firstMonth, lastMonth);
        var result = await _financialTransactionService.GetNetProfitGrowthAsync(userId.Value, fMonth, lMonth);
        return Ok(result);
    }

    [HttpGet("spending-comparison")]
    [ProducesResponseType(typeof(SpendingComparison), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SpendingComparison>> GetSpendingComparison(
        [FromQuery] string? firstMonth = null,
        [FromQuery] string? lastMonth = null
    )
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized("O usuário não possui as credências necessárias");
        var (fMonth, lMonth) = ParseDateRange(firstMonth, lastMonth);
        var result = await _financialTransactionService.GetSpendingComparisonAsync(userId.Value, fMonth, lMonth);
        return Ok(result);
    }

    [HttpGet("month-present")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentMonthSpendings(
        [FromQuery] string? firstMonth = null,
        [FromQuery] string? lastMonth = null
    )
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized("O usuário não possui as credências necessárias");
        var (fMonth, lMonth) = ParseDateRange(firstMonth, lastMonth);
        var result = await _financialTransactionService.GetCurrentMonthSpendingsAsync(
            userId.Value, fMonth, lMonth);
        return Ok(result);
    }

    [HttpGet("top-earnings")]
    [ProducesResponseType(typeof(TopEarningMonth), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<TopEarningMonth>>> GetTopEarningMonths()
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized("O usuário não possui as credências necessárias");
        var result = await _financialTransactionService.GetTopEarningMonthAsync(userId.Value);
        if (result.Count == 0) return NotFound("Nenhuma receita encontrada para o usuário.");
        return Ok(result);
    }

    [HttpGet("top-savings")]
    [ProducesResponseType(typeof(TopSavingsMonth), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<TopSavingsMonth>>> GetTopSavingsMonths()
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized("O usuário não possui as credências necessárias");
        var result = await _financialTransactionService.GetTopSavingsMonthAsync(userId.Value);
        if (result.Count == 0) return NotFound("Nenhuma economia registrada entre meses.");
        return Ok(result);
    }

    [HttpGet("current-month-spenging")]
    [ProducesResponseType(typeof(SpendingSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SpendingSummaryDto>> GetCurrentMonthSpendingsReport()
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            var response = ApiResponse<string>.CreateFailure("Credenciais invalidas");
            return Unauthorized(response);
        }

        var result = await _financialTransactionService.GetCurrentMonthSpendingSummaryAsync(userId.Value);
        if (result == null)
        {
            var response = ApiResponse<string>.CreateFailure("Nenhum gasto encontrado para o mês atual.");
            return NotFound(response);
        }

        var successResponse = ApiResponse<SpendingSummaryDto>.CreateSuccess(result);
        return Ok(successResponse);
    }
}