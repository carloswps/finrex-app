using System.ComponentModel.DataAnnotations;

namespace Finrex_App.Core.DTOs;

public class RegisterDTO
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
    public string Nome { get; set; }

    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; }

    [Required(ErrorMessage = "A senha é obrigatória")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 100 caracteres")]
    public string Senha { get; set; }
    
    [Compare("Senha", ErrorMessage = "As senhas não conferem")]
    public string ConfirmarSenha { get; set; }
}
