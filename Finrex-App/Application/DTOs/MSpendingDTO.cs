namespace Finrex_App.Application.DTOs;

public class MSpendingDtO
{
    public DateOnly Date { get; set; }
    public decimal? Transportation { get; set; }
    public decimal? Entertainment { get; set; }
    public decimal? Rent { get; set; }
    public decimal? Groceries { get; set; }
    public decimal? Utilities { get; set; }
}