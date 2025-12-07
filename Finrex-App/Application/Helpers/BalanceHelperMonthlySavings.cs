using Finrex_App.Domain.Entities;

namespace Finrex_App.Application.Helpers;

internal static class BalanceHelperMonthlySavings
{
    internal static List<TopSavingsMonth> CalculateMonthlySavings(List<(DateOnly Month, decimal TotalSpending)> allMonthlySpendings)
    {
        var savingsList = new List<TopSavingsMonth>();

        for (int i = 1; i < allMonthlySpendings.Count; i++)
        {
            var currentMonthData = allMonthlySpendings[i];

            var previousMonthData = allMonthlySpendings[i - 1];

            decimal currentSpending = currentMonthData.TotalSpending;
            decimal previousSpending = previousMonthData.TotalSpending;

            decimal savingsInReais = previousSpending - currentSpending;
            decimal savingsInPercentage = previousSpending > 0 ? (savingsInReais / previousSpending) * 100 : 0;

            savingsList.Add(new TopSavingsMonth
            {
                Month = currentMonthData.Month,
                SavingsPercentage = Math.Round(savingsInPercentage, 2),
                SavingsInReais = savingsInReais
            });
        }
        return savingsList;
    }
}