using System.Security.Claims;

namespace Realworlddotnet.Infrastructure.Extensions.Authentication;

public static class ClaimsPrincipalExtension
{
    public static string GetUsername(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    }
}
