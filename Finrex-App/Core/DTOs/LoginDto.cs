using System.ComponentModel.DataAnnotations;

namespace Finrex_App.Core.DTOs;

public class LoginDto
{
    public int Id { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [MinLength( 8 )]
    public required string Senha { get; set; }
}