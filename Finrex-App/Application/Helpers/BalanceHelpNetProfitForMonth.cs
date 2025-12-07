using Finrex_App.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Finrex_App.Application.Helpers;

internal static class BalanceHelpNetProfitForMonth
{
    internal static async Task<decimal> CalculateNetProfitForMonth(
        int userId, DateOnly month, AppDbContext context )
    {
        var monthStart = month;
        var monthEnd = month.AddMonths( 1 ).AddDays( -1 );

        var totalRevenue = await context.MIncome
            .Where( i => i.UsuarioId == userId && i.Date >= monthStart && i.Date <= monthEnd )
            .SumAsync( i => i.MainIncome + i.Freelance + i.Benefits + i.BusinessProfit + i.Other );

        var operatingExpenses = await context.MSpending
            .Where( s => s.UsuarioId == userId && s.Date >= monthStart && s.Date <= monthEnd )
            .SumAsync( s => s.Transportation + s.Groceries + s.Entertainment + s.Rent + s.Utilities );

        var taxAndInterestDate = await context.MFinanceFactorsEnumerable
            .Where( f => f.UsuarioId == userId && f.Date >= monthStart && f.Date <= monthEnd )
            .Select( f => new { TotalTaxes = f.Taxes, TotalInterest = f.Interest } )
            .FirstOrDefaultAsync() ?? new { TotalTaxes = 0M, TotalInterest = 0M };

        var totalTaxes = taxAndInterestDate.TotalTaxes;
        var totalInterest = taxAndInterestDate.TotalInterest;

        var totalDeductions = operatingExpenses + totalTaxes + totalInterest;

        return totalRevenue - totalDeductions;
    }
}