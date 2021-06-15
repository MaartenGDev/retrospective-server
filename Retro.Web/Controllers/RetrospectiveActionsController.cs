using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Retro.Data.Context;
using Retro.Data.Models;
using Retro.Web.Authentication;
using Retro.Web.Models;
using Action = Retro.Data.Models.Action;

namespace Retro.Web.Controllers
{
    [Authorize]
    [ApiController]
    public class RetrospectiveActionsController : ControllerBase {
        private readonly RetroContext _dbContext;

        public RetrospectiveActionsController(RetroContext retroContext)
        {
            _dbContext = retroContext;
        }

        [HttpPost("retrospectives/{id}/actions")]
        public ActionResult<Action> Create(int id, [FromBody] Action action)
        {
            var retrospective = _dbContext.Retrospectives.Single(r => r.Id == id);
            var teamMember = GetTeamMemberForTeam(retrospective.TeamId, User.GetId());
            
            if (teamMember == null || !teamMember.Role.CanManageRetrospective)
            {
                return Unauthorized();
            }

            action.RetrospectiveId = id;

            _dbContext.Actions.Add(action);
            _dbContext.SaveChanges();

            return action;
        }
        
        [HttpPatch("retrospectives/{retrospectiveId}/actions/{actionId}")]
        public ActionResult<Action> Update(int retrospectiveId, int actionId, [FromBody] Action action)
        {
            var teamMember = GetTeamMemberFromRetrospectiveTeam(retrospectiveId);
            
            if (teamMember == null || !teamMember.Role.CanManageRetrospective)
            {
                return Unauthorized();
            }

            var actionToUpdate = _dbContext.Actions.Single(a => a.Id == actionId);
            var teamMemberFromAction = GetTeamMemberFromRetrospectiveTeam(actionToUpdate.RetrospectiveId);

            if (teamMemberFromAction == null || !teamMemberFromAction.Role.CanManageRetrospective)
            {
                return Unauthorized();
            }

            actionToUpdate.Description = action.Description;
            actionToUpdate.Responsible = action.Responsible;
            
            _dbContext.SaveChanges();

            return action;
        }

        [HttpPatch("retrospectives/{retrospectiveId}/actions/{actionId}/completed")]
        public ActionResult<Action> CompleteAction(int retrospectiveId, int actionId)
        {
            var retrospective = _dbContext.Retrospectives.Single(r => r.Id == retrospectiveId);
            var teamMember = GetTeamMemberForTeam(retrospective.TeamId, User.GetId());
            
            if (teamMember == null || !teamMember.Role.CanManageRetrospective)
            {
                return Unauthorized();
            }

            var actionToUpdate = _dbContext.Actions.Single(a => a.Id == actionId);
            var teamMemberFromAction = GetTeamMemberFromRetrospectiveTeam(actionToUpdate.RetrospectiveId);

            if (teamMemberFromAction == null || !teamMemberFromAction.Role.CanManageRetrospective)
            {
                return Unauthorized();
            }

            actionToUpdate.IsCompleted = true;
            
            _dbContext.SaveChanges();

            return actionToUpdate;
        }
        
        private TeamMember GetTeamMemberFromRetrospectiveTeam(int retrospectiveId)
        {
            var retrospective = _dbContext.Retrospectives.Single(r => r.Id == retrospectiveId);
            return GetTeamMemberForTeam(retrospective.TeamId, User.GetId());
        }
        
        private TeamMember GetTeamMemberForTeam(int teamId, string userId)
        {
            return _dbContext.TeamMembers.Include(tm => tm.Role)
                .SingleOrDefault(tm => tm.TeamId == teamId && tm.UserId == userId);
        }
    }
}