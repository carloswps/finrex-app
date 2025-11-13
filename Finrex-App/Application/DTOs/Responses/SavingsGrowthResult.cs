namespace Finrex_App.Application.DTOs;

public class SavingsGrowthResult
{
    public DateOnly FirstMonth { get; set; }
    public DateOnly LastMonth { get; set; }

    public decimal FirstMonthNetBalance { get; set; }
    public decimal LastMonthNetBalance { get; set; }

    public decimal GrowInReais { get; set; }

    public decimal GrowInPorcentage { get; set; }
}