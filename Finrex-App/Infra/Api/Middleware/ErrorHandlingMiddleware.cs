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
public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException validationException)
        {
            _logger.LogWarning(validationException, "Validation exception caught by middleware.");
            var statusCode = context.Response.StatusCode;
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";

                var erros = validationException.Errors.Select(e => new
                {
                    Campo = e.PropertyName,
                    Mensagem = e.ErrorMessage
                });

                var errorResponse = new
                {
                    error = "validation_error",
                    message = "One or more validation errors occurred.",
                    details = erros,
                    statusCode = statusCode
                };

                await context.Response.WriteAsJsonAsync(errorResponse);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unhandled exception caught by middleware. Request Path: {Path}",
                context.Request.Path);
            var statusCode = context.Response.StatusCode;
            if (statusCode < 400) statusCode = (int)HttpStatusCode.InternalServerError;

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";

                var errorResponse = new
                {
                    error = "error",
                    message = HandleGetErrorMessage(statusCode),
                    details = statusCode == (int)HttpStatusCode.InternalServerError
                        ? "An a internal server error occurred. Please try again later."
                        : null,
                    statusCode = statusCode
                };

                await context.Response.WriteAsJsonAsync(errorResponse);
            }
            else
            {
                _logger.LogWarning(
                    "Response already started, cannot write standardized error response for status code {StatusCode}. Exception: " +
                    "{ExceptionMessage}", statusCode, e.Message);
            }
        }
    }

    private string HandleGetErrorMessage(int statusCode)
    {
        return statusCode switch
        {
            (int)HttpStatusCode.Unauthorized => "Authentication failed. Please check your credentials.",
            (int)HttpStatusCode.Forbidden => "You do not have permission to access this resource.",
            (int)HttpStatusCode.NotFound => "The requested resource was not found.",
            (int)HttpStatusCode.BadRequest => "The request was invalid. Please check your input.",
            (int)HttpStatusCode.InternalServerError => "An unexpected server error occurred. Please try again later.",
            _ => "An unknown error occurred."
        };
    }
}