using System.ComponentModel.DataAnnotations;

namespace Finrex_App.Core.Entities;

/// <summary>
/// Represents a user within the Finrex application.
/// </summary>
/// <remarks>
/// This entity is used to represent user data and includes several properties
/// like Id, Nome, Email, Senha, CriadoEm, and AtualizadoEm.
/// The Nome, Email, and Senha properties are required and have specific
/// validation constraints.
/// </remarks>
public class User
{
    public int Id { get; set; }

    [Required( ErrorMessage = "O nome é obrigatório" )]
    [StringLength( 100, ErrorMessage = "O nome deve ter no máximo 100 caracteres" )]
    public string Nome { get; set; }

    [Required( ErrorMessage = "O email é obrigatório" )]
    [EmailAddress( ErrorMessage = "Email inválido" )]
    public string Email { get; set; }

    [Required( ErrorMessage = "A senha é obrigatória" )]
    public string Senha { get; set; }

    public DateTime CriadoEm { get; set; }
    public DateTime AtualizadoEm { get; set; }
}