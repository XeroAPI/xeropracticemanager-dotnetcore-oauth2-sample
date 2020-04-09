using System.Security.Claims;

namespace XeroPracticeManagerOAuth2Sample.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string XeroUserId(this ClaimsPrincipal claims)
        {
            return claims.FindFirstValue("xero_userid");
        }
    }
}
