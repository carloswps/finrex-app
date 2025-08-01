using System.Net;
using System.Text.Json;
using FluentValidation;

namespace Finrex_App.Infra.Api.Middleware;

/// <summary>
/// Represents a middleware component for handling exceptions during the HTTP request processing pipeline.
/// </summary>
/// <remarks>
/// This middleware handles unhandled exceptions that occur during the execution of any subsequent middleware.
/// It logs the exception details and provides a centralized point to manage error responses.
/// </remarks>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware( RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger )
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync( HttpContext context )
    {
        try
        {
            await _next( context );
        } catch ( Exception e )
        {
            _logger.LogError( e, "Ocorreu um erro não tratado" );
            await HandleExceptionAsync(context, e);
        }
    }

    private static Task HandleExceptionAsync( HttpContext context, Exception exception )
    {
        // Treating error validations fluentValidation
        context.Response.ContentType = "application/json";

        if ( exception is ValidationException validationException )
        {
            var error = validationException.Errors.Select( e => new
            {
                Campo = e.PropertyName,
                Erro = e.ErrorMessage
            } );

            var response = new
            {
                Sucesso = false,
                Erros = error,
                Message = "Erro de validação"
            };
            return context.Response.WriteAsJsonAsync( response  );
        }
        
        // Treating generic errors
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        var genericResponse = new
        {
            status = context.Response.StatusCode,
            message = "Ocorreu um erro ao processar a requisição. ",
            detailedMessage = exception.Message
        };
        return context.Response.WriteAsJsonAsync( genericResponse  );
    }
}