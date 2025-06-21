using System.ComponentModel.DataAnnotations;

namespace Finrex_App.DTOS;

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string email { get; set; }

    [Required]
    [MinLength( 8 )]
    public string senha { get; set; }
}

public class LoginResponseDto
{
    public int id { get; set; }
    public string email { get; set; } = string.Empty;
    public string senha { get; set; } = string.Empty;
}