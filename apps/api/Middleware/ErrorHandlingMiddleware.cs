using Api.Models;
using FluentValidation;
using System.Net;
using System.Text.Json;

namespace Api.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ErrorResponse
        {
            TraceId = context.TraceIdentifier
        };

        switch (exception)
        {
            case ValidationException validationEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Error = "ValidationFailed";
                errorResponse.Message = string.Join("; ", validationEx.Errors.Select(e => e.ErrorMessage));
                break;

            case ArgumentException _:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Error = "BadRequest";
                errorResponse.Message = exception.Message;
                break;

            case FileNotFoundException _:
            case DirectoryNotFoundException _:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Error = "NotFound";
                errorResponse.Message = "The requested resource was not found";
                break;

            case UnauthorizedAccessException _:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Error = "Unauthorized";
                errorResponse.Message = "Access denied";
                break;

            case TimeoutException _:
                response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                errorResponse.Error = "Timeout";
                errorResponse.Message = "The request timed out";
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Error = "InternalError";
                errorResponse.Message = "An internal server error occurred";
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(jsonResponse);
    }
}
