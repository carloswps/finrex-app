namespace Finrex_App.Application.DTOs;

public class TopEarningMonth
{
    public DateOnly Month { get; set; }
    public decimal TotalRevenue { get; set; }
}