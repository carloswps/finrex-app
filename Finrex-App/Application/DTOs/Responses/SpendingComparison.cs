namespace Finrex_App.Application.DTOs;

public class SpendingComparison
{
    public DateOnly FirstMonth { get; set; }
    public DateOnly LastMonth { get; set; }

    public decimal FirstSpendingMonth { get; set; }
    public decimal LastSpendingMonth { get; set; }

    public decimal DifferenceBetweenMonths { get; set; }
    public decimal DifferenceInPorcentage { get; set; }
}