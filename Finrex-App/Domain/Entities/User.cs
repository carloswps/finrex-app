namespace Finrex_App.Domain.Entities;

/// <summary>
/// Represents a user within the Finrex application.
/// </summary>
/// <remarks>
/// This entity is used to represent user data and includes several properties
/// like Id, Email, Senha, CriadoEm, and AtualizadoEm.
/// The Email, and Senha properties are required and have specific
/// validation constraints.
/// </remarks>
public class User
{
    public int Id { get; set; }
    public string email { get; set; } = "";
    public string password { get; set; } = "";


    // Navigation Properties
    public ICollection<MonthlySpending>? MonthlySpendings { get; set; }
    public ICollection<MonthlyIncome>? MonthlyIncomes { get; set; }
    public ICollection<MFinanceFactors>? MFinanceFactorsCollection { get; set; }
}