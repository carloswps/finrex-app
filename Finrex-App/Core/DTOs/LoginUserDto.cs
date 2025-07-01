using System.ComponentModel.DataAnnotations;

namespace Finrex_App.Core.DTOs;

public class LoginUserDto
{
    [Required( ErrorMessage = "Porfavor infome o seu email" )]
    [EmailAddress( ErrorMessage = "Porfavor informe um email valido" )]
    public string Email { get; set; }

    [Required( ErrorMessage = "Porfavor infome a sua senha" )]
    public string Senha { get; set; }

    [Compare( "Senha", ErrorMessage = "As senhas devem ser iguais" )]
    public string ConfirmarSenha { get; set; }
}