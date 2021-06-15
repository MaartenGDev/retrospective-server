using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Retro.Data.Context;
using Retro.Data.Models;
using Retro.Web.Authentication;

namespace Retro.Web.Services
{
    public class ApplicationUserService
    {
        private readonly RetroContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ApplicationUserService(RetroContext dbContext, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task RefreshClaims(ApplicationUser user)
        {
            
            var existingClaims = await _userManager.GetClaimsAsync(user);
            await _userManager.RemoveClaimsAsync(user, existingClaims);

            var teamsOfUser = _dbContext.TeamMembers.Include(tm => tm.Role).Where(tm => tm.UserId == user.Id).ToList();
            var teamIdsWhereUserIsMember = teamsOfUser.Select(tm => tm.TeamId).ToList();

            await _userManager.AddClaimsAsync(user,
                teamIdsWhereUserIsMember.Select(teamId => new Claim(UserClaims.MEMBER_OF_TEAM_ID, teamId.ToString())));
            
            await _signInManager.RefreshSignInAsync(user);
        }

        public async Task RefreshClaims(ClaimsPrincipal principal)
        {
            var user = await _userManager.FindByNameAsync(principal.FindFirst(ClaimTypes.Name).Value);

            await RefreshClaims(user);
        }
    }
}