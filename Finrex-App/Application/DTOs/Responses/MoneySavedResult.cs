namespace Finrex_App.Application.DTOs;

public class MoneySavedResult
{
    public DateOnly FirstMonth { get; set; }
    public DateOnly LastMonth { get; set; }

    public decimal FirstMonthSaveSpending { get; set; }
    public decimal LastMonthSaveSpending { get; set; }

    public decimal DifferenceBetweenMonths { get; set; }
    public decimal DifferenceInPorcentage { get; set; }
}