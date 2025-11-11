namespace Finrex_App.Application.DTOs;

public class MIncomeDto
{
    public DateOnly Date { get; set; }
    public decimal? MainIncome { get; set; }
    public decimal? Freelance { get; set; }
    public decimal? Benefits { get; set; }
    public decimal? BusinessProfit { get; set; }
    public decimal? Other { get; set; }
}