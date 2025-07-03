using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

/// <summary>
/// Operation filter para o Swagger que aplica valores padrão de descrição aos parâmetros
/// com base nos metadados da API (como ModelMetadata).
/// </summary>
/// <remarks>
/// Este filtro é útil para preencher automaticamente as descrições dos parâmetros
/// nos endpoints do Swagger, caso não sejam fornecidas manualmente.
/// </remarks>
public class SwaggerDefaultValues : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null)
        {
            return;
        } 

        foreach (var parameter in operation.Parameters)
        {
            var description = context.ApiDescription.ParameterDescriptions
                .FirstOrDefault(p => p.Name == parameter.Name);

            if (description != null && parameter.Description == null)
            {
                parameter.Description = description.ModelMetadata?.Description;
            }
        }
    }
}