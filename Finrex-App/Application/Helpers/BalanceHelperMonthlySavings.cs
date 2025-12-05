using Finrex_App.Domain.Entities;

namespace Finrex_App.Application.Helpers;

internal static class BalanceHelperMonthlySavings
{
    internal static List<TopSavingsMonth> CalculateMonthlySavings(List<(DateOnly Month, decimal TotalSpending)> allMonthlySpendings)
    {
        var savingsList = new List<TopSavingsMonth>();

        for (int i = 0; i < allMonthlySpendings.Count; i++)
        {
            var currentMonthData = allMonthlySpendings[i];
            if (i > 0) // Ensure there's a previous month to compare with
            {
                var previousMonthData = allMonthlySpendings[i - 1];

                decimal currentSpending = currentMonthData.TotalSpending;
                decimal previousSpending = previousMonthData.TotalSpending;

                decimal savingsInReais = previousSpending - currentSpending;
                decimal savingsInPercentage = 0;

                if (savingsInReais > 0 && previousSpending > 0)
                {
                    savingsInPercentage = (savingsInReais / previousSpending) * 100;
                    savingsInPercentage = Math.Round(savingsInPercentage, 2);

                    savingsList.Add(new TopSavingsMonth
                    {
                        Month = currentMonthData.Month,
                        SavingsPercentage = savingsInPercentage,
                        SavingsInReais = savingsInReais
                    });
                }
            }
        }
        return savingsList;
    }
}