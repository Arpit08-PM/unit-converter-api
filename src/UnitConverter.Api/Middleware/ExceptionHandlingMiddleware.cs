using System.Net;
using System.Text.Json;
using UnitConverter.Api.Exceptions;

namespace UnitConverter.Api.Middleware;

/// <summary>
/// Catches known domain exceptions and maps them to appropriate HTTP problem responses.
/// This keeps exception-handling logic out of controllers.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next   = next;
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = exception switch
        {
            UnitNotFoundException e         => (HttpStatusCode.BadRequest,    "Unit Not Found",           e.Message),
            IncompatibleUnitsException e    => (HttpStatusCode.BadRequest,    "Incompatible Units",       e.Message),
            _                              => (HttpStatusCode.InternalServerError, "Internal Server Error", "An unexpected error occurred.")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception");

        context.Response.StatusCode  = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new
        {
            type     = $"https://httpstatuses.com/{(int)statusCode}",
            title,
            status   = (int)statusCode,
            detail,
            instance = context.Request.Path.Value,
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
    }
}
