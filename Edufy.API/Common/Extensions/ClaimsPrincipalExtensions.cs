using System.Security.Claims;

namespace Edufy.API.Common.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new UnauthorizedAccessException("UserId claim not found");

        return Guid.TryParse(value, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("UserId claim is invalid");
    }

    public static string GetEmail(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Email)
               ?? throw new UnauthorizedAccessException("Email claim not found");
    }

    public static string GetFullName(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Name)
               ?? throw new UnauthorizedAccessException("FullName claim not found");
    }

    public static string GetRole(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Role)
               ?? throw new UnauthorizedAccessException("Role claim not found");
    }

    public static string GetSessionId(this ClaimsPrincipal user)
    {
        return user.FindFirstValue("session_id")
               ?? throw new UnauthorizedAccessException("SessionId claim not found");
    }

    public static bool IsInRole(this ClaimsPrincipal user, string role)
    {
        return user.HasClaim(ClaimTypes.Role, role);
    }
}