using System.ComponentModel.DataAnnotations;

namespace Finrex_App.Core.DTOs;

public class LoginUserDto
{ 
    public string Email { get; set; }
    public string Senha { get; set; }
}