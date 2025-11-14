using Finrex_App.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Finrex_App.Application.Helpers;

internal static class BalanceHelperForMonth
{
    internal static async Task<decimal> CalculateNetBalanceForMonth( int userId, DateOnly month, AppDbContext context )
    {
        var monthStart = month;
        var monthEnd = month.AddMonths( 1 ).AddDays( -1 );

        var totalIncome = await context.MIncome
            .Where( i => i.UsuarioId == userId && i.Date >= monthStart && i.Date <= monthEnd )
            .SumAsync( i => i.MainIncome + i.Freelance + i.Benefits + i.BusinessProfit + i.Other );

        var totalSpending = await context.MSpending
            .Where( s => s.UsuarioId == userId && s.Date >= monthStart && s.Date <= monthEnd )
            .SumAsync( s => s.Groceries + s.Rent + s.Transportation + s.Utilities + s.Entertainment );

        return totalIncome - totalSpending;
    }
}