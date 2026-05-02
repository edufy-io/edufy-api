using Microsoft.AspNetCore.Mvc;

namespace Edufy.API.Middlewares;

public class ExceptionMiddleware(
    RequestDelegate next,
    ILogger<ExceptionMiddleware> logger,
    IHostEnvironment env
)
{
    private const string UnexpectedErrorMessage = "An unexpected error occurred";

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    async private Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var correlationId = context.Items["X-Correlation-Id"]?.ToString() ?? Guid.NewGuid().ToString();

        logger.LogError(
            ex,
            "Unexpected error. CorrelationId: {CorrelationId}, Path: {Path}, Method: {Method}",
            correlationId,
            context.Request.Path,
            context.Request.Method
        );

        var (statusCode, problem) = BuildProblemDetails(context, ex, correlationId);

        context.Response.StatusCode  = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problem);
    }

    private (int statusCode, ProblemDetails problem) BuildProblemDetails(
        HttpContext context,
        Exception ex,
        string correlationId)
    {
        var problem = new ProblemDetails
        {
            Type     = "https://tools.ietf.org/html/rfc7807",
            Status   = StatusCodes.Status500InternalServerError,
            Title    = "Internal Server Error",
            Detail   = env.IsProduction() ? UnexpectedErrorMessage : ex.Message,
            Instance = context.Request.Path
        };

        problem.Extensions["correlationId"] = correlationId;
        problem.Extensions["traceId"]       = context.TraceIdentifier;

        if (!env.IsProduction())
        {
            problem.Extensions["stackTrace"] = ex.StackTrace;
        }

        return (StatusCodes.Status500InternalServerError, problem);
    }
}