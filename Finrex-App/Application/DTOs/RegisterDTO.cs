namespace Finrex_App.Application.DTOs;

public class RegisterDTO
{
    public string Nome { get; set; }
    
    public string Email { get; set; }
    
    public string Senha { get; set; }
    
    public string ConfirmarSenha { get; set; }
}