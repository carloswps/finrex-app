using Finrex_App.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Finrex_App.Application.Helpers;

internal static class BalanceHelperForSpendingCompare
{
    internal static async Task<decimal> CalculateBalanceSpending( int userId, DateOnly month, AppDbContext context )
    {
        var monthStart = month;
        var monthEnd = month.AddMonths( 1 ).AddDays( -1 );

        var currentMonthSpending = await context.MSpending
            .Where( s => s.UsuarioId == userId && s.Date >= monthStart && s.Date <= monthEnd )
            .SumAsync( s => s.Transportation + s.Entertainment + s.Rent + s.Groceries + s.Utilities );

        return currentMonthSpending;
    }
}