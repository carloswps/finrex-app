using System.Security.Claims;

namespace Finrex_App.Extensions;

public static class ClaimsExtensions
{
    public static int? GetUserId(this ClaimsPrincipal user)
    {
        if (user == null) return null;
        var userIdString = user.FindFirst("userId")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdString, out var userId)) return userId;
        return null;
    }
}