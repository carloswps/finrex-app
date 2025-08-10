using Finrex_App.Application.DTOs;
using Finrex_App.Core.DTOs;
using Swashbuckle.AspNetCore.Filters;

namespace Finrex_App.Core.Example;

/// <summary>
/// Provides example data for the RegisterDTO class.
/// </summary>
/// <remarks>
/// This class implements the IExamplesProvider interface to return sample data for the RegisterDTO model.
/// Useful for generating examples in API documentation during runtime using tools like Swagger.
/// </remarks>
public abstract class RegisterDtoExample : IExamplesProvider<RegisterDTO>
{
    /// <summary>
    /// Provides an example instance of the RegisterDTO object for use in API documentation or testing.
    /// </summary>
    /// <returns>
    /// An instance of the RegisterDTO class containing sample data, including values for Nome, Email, Senha, and ConfirmarSenha.
    /// </returns>
    public RegisterDTO GetExamples()
    {
        return new RegisterDTO
        {
            Email = "joaquimmiquelteste@gmail.com",
            Senha = "Senha123"
        };
    }
}