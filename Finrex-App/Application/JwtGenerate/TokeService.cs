using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Finrex_App.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace Finrex_App.Application.JwtGenerate;

/// <summary>
/// Provides services for generating JSON Web Tokens (JWT) for user authentication and authorization.
/// </summary>
public class TokeService
{
    private readonly IConfiguration _configuration;

    public TokeService( IConfiguration configuration )
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Generates a JSON Web Token (JWT) for the specified user.
    /// </summary>
    /// <param name="user">The user for whom the token needs to be generated. This includes user-specific data required for the token creation.</param>
    /// <returns>A JWT as a string that can be used for user authentication and authorization, or null if token generation fails.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the necessary configuration for token generation, such as the signing key, issuer, or audience, is missing or invalid.</exception>
    public string? GenerateToken( User user, string authProvider )
    {
        var key = Encoding.UTF8.GetBytes( _configuration[ "Jwt:Key" ] ??
                                          throw new InvalidOperationException( "Nenhuma chave encontrada" ) );

        var tokenConfig = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity( new Claim[]
            {
                new( ClaimTypes.NameIdentifier, user.Id.ToString() ),
                new( ClaimTypes.Email, user.email ),
                new( JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString() ),
                new( "auth_provider", authProvider )
            } ),
            Expires = DateTime.UtcNow.AddHours( 8 ),
            Issuer = _configuration[ "Jwt:Issuer" ],
            Audience = _configuration[ "Jwt:Audience" ],
            SigningCredentials = new SigningCredentials( new SymmetricSecurityKey( key ),
                SecurityAlgorithms.HmacSha256Signature )
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken( tokenConfig );
        var TokenString = tokenHandler.WriteToken( token );
        return TokenString;
    }
}