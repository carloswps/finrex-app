namespace Finrex_App.Application.DTOs;

public class MIncomeDto
{
    public int Mes { get; set; }
    public decimal? MainIncome { get; set; }
    public decimal? Freelance { get; set; }
    public decimal? Benefits { get; set; }
    public decimal? BussinesProfit { get; set; }
    public decimal? Other { get; set; }
}