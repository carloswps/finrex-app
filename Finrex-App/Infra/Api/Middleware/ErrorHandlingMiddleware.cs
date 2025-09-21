using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

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
        var originalBodyStream = context.Response.Body;
        try
        {
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next( context );
            if ( context.Response.StatusCode == ( int )HttpStatusCode.Unauthorized ||
                 context.Response.StatusCode == ( int )HttpStatusCode.Forbidden )
            {
                var response = new
                {
                    Success = false,
                    Error = context.Response.StatusCode,
                    Message
                        = "Você não possui permissão para acessar este recurso. Verifique suas credenciais do sistema."
                };

                context.Response.ContentType = "application/json";
                var jsonResponse = JsonSerializer.Serialize( response );

                responseBody.SetLength( 0 );
                await responseBody.WriteAsync( Encoding.UTF8.GetBytes( jsonResponse ).AsMemory() );
                await responseBody.FlushAsync();

                responseBody.Position = 0;
                await responseBody.CopyToAsync( originalBodyStream );
            } else
            {
                responseBody.Position = 0;
                await responseBody.CopyToAsync( originalBodyStream );
            }
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

            context.Response.Body = originalBodyStream;
            await HandleExceptionAsync( context, e );
        } finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private static Task HandleExceptionAsync( HttpContext context, Exception exception )
    {
        // Treating error validations fluentValidation
        context.Response.ContentType = "application/json";

        int statusCode;
        object response;

        switch ( exception )
        {
            case UnauthorizedAccessException:
            {
                statusCode = ( int )HttpStatusCode.Unauthorized;
                response = new
                {
                    Success = false,
                    Error = statusCode,
                    Message = "Acesso não autorizado. Por favor, faça login novamente."
                };
                break;
            }
            case ValidationException:
            {
                statusCode = ( int )HttpStatusCode.BadRequest;

                response = new
                {
                    Success = false,
                    Error = statusCode,
                    Message = "Ocorreu um erro de validação. Por favor, verifique os campos e tente novamente."
                };
                break;
            }
            default:
            {
                statusCode = ( int )HttpStatusCode.InternalServerError;

                response = new
                {
                    Success = false,
                    Error = statusCode,
                    Message = "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde."
                };
                break;
            }
        }


        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync( response );
    }
}