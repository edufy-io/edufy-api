namespace Edufy.Shared.Common;

public record Error(ErrorCode Code, string Message)
{
    public static readonly Error None = new Error(ErrorCode.Unexpected, string.Empty);

    public static Error NotFound(string message) => new Error(ErrorCode.NotFound, message);
    public static Error Conflict(string message) => new Error(ErrorCode.Conflict, message);
    public static Error Unauthorized(string message) => new Error(ErrorCode.Unauthorized, message);
    public static Error Forbidden(string message)     => new Error(ErrorCode.Forbidden, message);
    public static Error Validation(string message)    => new Error(ErrorCode.Validation, message);
    public static Error Unexpected(string message)    => new Error(ErrorCode.Unexpected, message);
}