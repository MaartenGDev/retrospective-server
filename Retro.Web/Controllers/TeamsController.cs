using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Retro.Data.Context;
using Retro.Data.Models;
using Retro.Web.Authentication;
using Retro.Web.Models;
using Retro.Web.Services;

namespace Retro.Web.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class TeamsController : Controller
    {
        private readonly RetroContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ApplicationUserService _applicationUserService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TeamsController(RetroContext dbContext, IConfiguration configuration, ApplicationUserService applicationUserService, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _applicationUserService = applicationUserService;
            _userManager = userManager;
        }

        [HttpGet]
        public IEnumerable<Team> Get()
        {
            var userId = User.GetId();

            return _dbContext.Teams
                .Include(t => t.Members)
                .ThenInclude(m => m.User)
                .Include(t => t.Members)
                .ThenInclude(t => t.Role)
                .ToList()
                .Where(t => t.Members.Exists(m => m.UserId == userId));
        }
        
        [HttpPost]
        public async Task<Team> Post([FromBody] Team team)
        {
            var userId = User.GetId();

            var currentUser = await _userManager.GetUserAsync(User);
            
            team.InviteCode = Guid.NewGuid().ToString();
            team.Members = new List<TeamMember>
            {
                new TeamMember {UserId = userId, RoleId = TeamMemberRoleIdentifiers.Admin, User = currentUser}
            };
            await _dbContext.Teams.AddAsync(team);
            await _dbContext.SaveChangesAsync();
            await _applicationUserService.RefreshClaims(User);

            return team;
        }
        
        [HttpPatch("{id}")]
        public ActionResult<Team> Patch(int id, [FromBody] Team team)
        {
            var (persistedTeam, isValid, response) = ValidateManageAction(id);

            if (!isValid)
            {
                return response;
            }

            persistedTeam.Name = team.Name;
            persistedTeam.Members = persistedTeam.Members.Select(m =>
            {
                var changedMember = team.Members.FirstOrDefault(tm => tm.Id == m.Id);

                if (changedMember != null)
                {
                    m.RoleId = changedMember.RoleId;
                }

                return m;
            }).ToList();

            _dbContext.SaveChanges();
            return persistedTeam;
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Team>> Delete(int id)
        {
            var (team, hasPermission, response) = ValidateManageAction(id);

            if (!hasPermission)
            {
                return response;
            }

            _dbContext.Teams.Remove(team);
            await _dbContext.SaveChangesAsync();
            await _applicationUserService.RefreshClaims(User);

            return team;
        }

        [HttpGet("invites/{inviteCode}")]
        public async Task<ActionResult<string>> JoinTeam(string inviteCode)
        {
            var team = _dbContext.Teams.Include(t => t.Members).FirstOrDefault(t => t.InviteCode == inviteCode);

            if (team == null)
            {
                return NotFound();
            }
            
            var userId = User.GetId();

            if (User.IsMemberOfTeam(team.Id)) return RedirectToClientApp();
            
            team.Members.Add(new TeamMember {UserId = userId, RoleId = TeamMemberRoleIdentifiers.Member});
            
            await _dbContext.SaveChangesAsync();
            await _applicationUserService.RefreshClaims(User);

            return RedirectToClientApp();
        }

        [AllowAnonymous]
        [HttpGet("discover/{inviteCode}")]
        public ActionResult<Team> FindBy(string inviteCode)
        {
            var team = _dbContext.Teams.FirstOrDefault(t => t.InviteCode == inviteCode);

            if (team == null)
            {
                return NotFound(new ErrorResponse("Invalid invite code, check the provided code/url."));
            }

            return team;
        }
        
        private ActionResult RedirectToClientApp()
        {
            var clientUrl = _configuration.GetValue<string>("ApplicationUrl");

            return Redirect(clientUrl);
        }
        
        private (Team, bool, ObjectResult) ValidateManageAction(int teamId)
        {
            var team = _dbContext.Teams
                .Include(t => t.Members)
                .ThenInclude(m => m.User)
                .Include(t => t.Members)
                .ThenInclude(t => t.Role)
                .SingleOrDefault(t => t.Id == teamId);
            
            if (team == null)
            {
                return (null, false, NotFound(new ErrorResponse("Unknown team")));
            }

            var teamMember = _dbContext.TeamMembers.Include(tm => tm.Role)
                .SingleOrDefault(tm => tm.TeamId == teamId && tm.UserId == User.GetId());
            
            if (teamMember == null || !teamMember.Role.CanManageTeam)
            {
                return (team, false, Unauthorized(new ErrorResponse($"Unauthorized, you are not able to perform actions for team {team.Name}")));
            }
            
            return (team, true, null);
        }
    }
}