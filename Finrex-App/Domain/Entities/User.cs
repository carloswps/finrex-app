using System.ComponentModel.DataAnnotations;

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
    public string Email { get; set; }
    public string Senha { get; set; }
    public DateOnly CriadoEm { get; set; }
    public DateOnly AtualizadoEm { get; set; }

    // Navigation Properties
    public ICollection<MonthlySpending> MonthlySpendings { get; set; }
    public ICollection<MonthlyIncome> MonthlyIncomes { get; set; }
}