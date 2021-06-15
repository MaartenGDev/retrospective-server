using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Retro.Web.Authentication
{
    public static class IdentityExtensions
    {
            public static bool IsMemberOfTeam(this ClaimsPrincipal principal, int teamId)
            {
                return GetTeams(principal).Any(id => teamId == id);
            }
            
            public static IEnumerable<int> GetTeams(this ClaimsPrincipal principal)
            {
                return principal.FindAll(UserClaims.MEMBER_OF_TEAM_ID).Select(c => int.Parse(c.Value));
            }
            
            public static string GetId(this ClaimsPrincipal principal)
            {
                return principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            }
    }
}