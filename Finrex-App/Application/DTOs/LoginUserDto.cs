using System.ComponentModel.DataAnnotations;

namespace Finrex_App.Core.DTOs;

public class LoginUserDto
{
    public string email { get; set; }
    public string password { get; set; }
}