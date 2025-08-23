namespace Finrex_App.Domain.Entities;

public class MonthlyIncome
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public DateOnly Date { get; set; }
    public decimal MainIncome { get; set; }
    public decimal Freelance { get; set; }
    public decimal Benefits { get; set; }
    public decimal BussinesProfit { get; set; }
    public decimal Other { get; set; }

    // Navigation Properties
    public User? User { get; set; }
}