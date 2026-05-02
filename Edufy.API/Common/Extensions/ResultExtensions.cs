using Edufy.Shared.Common;
using Microsoft.AspNetCore.Mvc;

namespace Edufy.API.Common.Extensions;

public static class ResultExtensions
{
    public static ProblemDetails ToProblemDetails(this Result result, HttpContext context)
    {
        var correlationId = context.Items["X-Correlation-Id"]?.ToString();

        var problem = new ProblemDetails
        {
            Type     = "https://tools.ietf.org/html/rfc7807",
            Title    = GetTitle(result.Error.Code),
            Status   = GetStatusCode(result.Error.Code),
            Detail   = result.Error.Message,
            Instance = context.Request.Path
        };

        problem.Extensions["correlationId"] = correlationId;
        problem.Extensions["traceId"]       = context.TraceIdentifier;

        return problem;
    }

    private static string GetTitle(ErrorCode code) => code switch
    {
        ErrorCode.NotFound     => "Not Found",
        ErrorCode.Conflict     => "Conflict",
        ErrorCode.Unauthorized => "Unauthorized",
        ErrorCode.Forbidden    => "Forbidden",
        ErrorCode.Validation   => "Validation Error",
        _                      => "Internal Server Error"
    };

    private static int GetStatusCode(ErrorCode code) => code switch
    {
        ErrorCode.NotFound     => StatusCodes.Status404NotFound,
        ErrorCode.Conflict     => StatusCodes.Status409Conflict,
        ErrorCode.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorCode.Forbidden    => StatusCodes.Status403Forbidden,
        ErrorCode.Validation   => StatusCodes.Status400BadRequest,
        _                      => StatusCodes.Status500InternalServerError
    };
}