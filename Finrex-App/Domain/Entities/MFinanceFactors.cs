namespace Finrex_App.Domain.Entities;

public class MFinanceFactors
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public DateOnly Date { get; set; }

    public decimal Taxes { get; set; } = 0;
    public decimal Interest { get; set; } = 0;
    public decimal OtherDeductions { get; set; } = 0;

    // Navigation Properties
    public User? User { get; set; }
}