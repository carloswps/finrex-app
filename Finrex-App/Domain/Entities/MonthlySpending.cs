namespace Finrex_App.Domain.Entities;

public class MonthlySpending
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public DateOnly Date { get; set; }
    public decimal Transportation { get; set; }
    public decimal Entertainment { get; set; }
    public decimal Rent { get; set; }
    public decimal Groceries { get; set; }
    public decimal Utilities { get; set; }

    // Navigation Properties
    public User User { get; set; }
}