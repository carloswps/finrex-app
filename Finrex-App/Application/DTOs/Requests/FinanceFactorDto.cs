namespace Finrex_App.Application.DTOs;

public class FinanceFactorDto
{
    public DateOnly? ReferenceDate { get; set; }

    public decimal? Taxes { get; set; }
    public decimal? Interest { get; set; }
    public decimal? OtherDeductions { get; set; }
}