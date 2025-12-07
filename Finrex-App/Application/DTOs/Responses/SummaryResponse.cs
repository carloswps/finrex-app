namespace Finrex_App.Application.DTOs;

public class SummaryResponse
{
    public IEnumerable<IncomeSummaryDto>? Income { get; set; }
    public IEnumerable<SpendingSummaryDto>? Spending { get; set; }
}

public class IncomeSummaryDto
{
    public string? Period { get; set; }
    public decimal? MainIncome { get; set; }
    public decimal? Freelance { get; set; }
    public decimal? Benefits { get; set; }
    public decimal? BusinessProfit { get; set; }
    public decimal? Other { get; set; }
}

public class SpendingSummaryDto
{
    public string? Period { get; set; }
    public decimal? Transportation { get; set; }
    public decimal? Entertainment { get; set; }
    public decimal? Rent { get; set; }
    public decimal? Groceries { get; set; }
    public decimal? Utilities { get; set; }
}