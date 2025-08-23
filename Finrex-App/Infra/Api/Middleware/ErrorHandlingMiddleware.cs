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
            SentrySdk.ConfigureScope( scope =>
            {
                scope.SetTag( "user", context.User.Identity?.Name );

                scope.SetTag( "method", context.Request.Method );
                scope.SetTag( "path", context.Request.Path );
                scope.SetTag( "host", context.Request.Host.Value );
                scope.SetTag( "protocol", context.Request.Protocol );
            } );

            SentrySdk.CaptureException( e );
            
            _logger.LogError( e, "Ocorreu um erro não tratado" );
            await HandleExceptionAsync( context, e );
        }
    }

    private static Task HandleExceptionAsync( HttpContext context, Exception exception )
    {
        // Treating error validations fluentValidation
        context.Response.ContentType = "application/json";

        int statusCode;
        object response;

        if ( exception is ValidationException validationException )
        {
            statusCode = ( int )HttpStatusCode.BadRequest;

            var errors = validationException.Errors.Select( e => new
            {
                Field = e.PropertyName,
                Error = e.ErrorMessage
            } );

            response = new
            {
                Sucesso = false,
                Erros = statusCode,
                Message = "Validation error occurred. Please check the fields and try again.",
                Errors = errors
            };
        } else if ( exception is UnauthorizedAccessException )
        {
            statusCode = ( int )HttpStatusCode.Unauthorized;
            response = new
            {
                Sucesso = false,
                Erros = statusCode,
                Message = "You are not authorized to access this resource. Please check your credentials and try again."
            };
        } else
        {
            statusCode = ( int )HttpStatusCode.InternalServerError;
            var detailedMessage = exception.Message;

            response = new
            {
                Success = false,
                ErrorCode = statusCode,
                Message = "An unexpected error occurred. Please try again later.",
                DetailedMessage = detailedMessage
            };
        }

        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync( response );
    }
}