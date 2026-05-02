using Serilog.Context;

namespace Edufy.API.Middlewares;

public class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrGenerateCorrelationId(context);

        SetCorrelationIdToRequest(context, correlationId);
        SetCorrelationIdToResponse(context, correlationId);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }

    private static string GetOrGenerateCorrelationId(HttpContext context)
    {
        return context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId)
            ? correlationId.ToString()
            : Guid.NewGuid().ToString();
    }

    private static void SetCorrelationIdToRequest(HttpContext context, string correlationId)
    {
        context.Items[CorrelationIdHeader] = correlationId;
    }
    
    private static void SetCorrelationIdToResponse(HttpContext context, string correlationId)
    {
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.TryAdd(CorrelationIdHeader, correlationId);
            return Task.CompletedTask;
        });
    }
}