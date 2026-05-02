using Microsoft.AspNetCore.Mvc;

namespace Edufy.API.Common.Extensions;

internal static class ValidationExtensions
{
    internal static IServiceCollection AddValidationConfiguration(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .ToDictionary(
                        k => k.Key,
                        v => v.Value!.Errors
                            .Select(e => e.ErrorMessage)
                            .ToArray()
                    );

                var correlationId = context.HttpContext
                    .Items["X-Correlation-Id"]?.ToString();

                var problem = new ValidationProblemDetails(errors)
                {
                    Type     = "https://tools.ietf.org/html/rfc7807",
                    Title    = "Validation Error",
                    Status   = StatusCodes.Status400BadRequest,
                    Detail   = "One or more validation errors occurred",
                    Instance = context.HttpContext.Request.Path
                };

                problem.Extensions["correlationId"] = correlationId;
                problem.Extensions["traceId"]       = context.HttpContext.TraceIdentifier;

                return new BadRequestObjectResult(problem);
            };
        });

        return services;
    }
}