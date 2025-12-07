namespace Finrex_App.Application.DTOs;

public class NetProfitResult
{
    public DateOnly FirstMonth { get; set; }
    public DateOnly LastMonth { get; set; }

    public decimal FirstMonthNetProfit { get; set; }
    public decimal LastMonthNetProfit { get; set; }

    public decimal VariationInReais { get; set; }

    public decimal VariationInPorcentage { get; set; }
}