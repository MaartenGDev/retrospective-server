using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Retro.Data.Context;
using Retro.Data.Models;
using Retro.Web.Authentication;
using Retro.Web.Extensions;
using Retro.Web.Models;
using Action = Retro.Data.Models.Action;

namespace Retro.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class RetrospectivesController : ControllerBase
    {
        private readonly ILogger<RetrospectivesController> _logger;
        private readonly RetroContext _dbContext;

        public RetrospectivesController(RetroContext retroContext, ILogger<RetrospectivesController> logger)
        {
            _logger = logger;
            _dbContext = retroContext;
        }

        [HttpGet]
        public IEnumerable<UserRetrospective> Get()
        {
            var evaluationsOfUser = _dbContext.Evaluations
                .Include(e => e.Comments)
                .ThenInclude(c => c.Category)
                .Include(e => e.TimeUsage)
                .Where(e => e.UserId == User.GetId())
                .ToList();

            var teamIdsOfUser = User.GetTeams();

            return _dbContext.Retrospectives
                .Include(r => r.Topics)
                .Include(r => r.Actions)
                .Include(r => r.Team)
                .Where(r => teamIdsOfUser.Contains(r.TeamId))
                .OrderByDescending(r => r.StartDate)
                .ToList()
                .Select(r =>
            {
                return new UserRetrospective
                {
                    Id = r.Id,
                    Name = r.Name,
                    Evaluation = evaluationsOfUser.FirstOrDefault(e => e.RetrospectiveId == r.Id),
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    Topics = r.Topics,
                    Actions = r.Actions,
                    Team = r.Team,
                };
            });
        }

        [HttpPost]
        public ActionResult<Retrospective> Post([FromBody] Retrospective retrospective)
        {
            var teamMember = GetTeamMemberForTeam(retrospective.TeamId, User.GetId());
            
            if (teamMember == null || !teamMember.Role.CanManageRetrospective)
            {
                return Unauthorized();
            }

            retrospective.StartDate = retrospective.StartDate.StartOfTheDay();
            retrospective.EndDate = retrospective.EndDate.EndOfTheDay();

            retrospective.Team = _dbContext.Teams.Find(retrospective.TeamId);
            _dbContext.Retrospectives.Add(retrospective);
            _dbContext.SaveChanges();

            return retrospective;
        }
        
        [HttpPatch("{id}")]
        public ActionResult<Retrospective> Patch(int id, [FromBody] Retrospective retrospective)
        {
            var persistedRetrospective = _dbContext.Retrospectives
                .Include(r => r.Topics)
                .Include(r => r.Actions)
                .SingleOrDefault(r => r.Id == id);

            if (persistedRetrospective == null)
            {
                return NotFound();
            }

            var teamMember = GetTeamMemberForTeam(persistedRetrospective.TeamId, User.GetId());
            
            if (teamMember == null || !teamMember.Role.CanManageRetrospective)
            {
                return Unauthorized();
            }

            var nextTopicIds = retrospective.Topics.Select(t => t.Id);
            var topicsToRemove = persistedRetrospective.Topics.Where(t => !nextTopicIds.Contains(t.Id));
            
            _dbContext.Topics.RemoveRange(topicsToRemove);
            
            var nextActionIds = retrospective.Actions.Select(t => t.Id);
            var actionsToRemove = persistedRetrospective.Actions.Where(t => !nextActionIds.Contains(t.Id));
            
            _dbContext.Actions.RemoveRange(actionsToRemove);

            persistedRetrospective.Name = retrospective.Name;
            persistedRetrospective.StartDate = retrospective.StartDate.StartOfTheDay();
            persistedRetrospective.EndDate = retrospective.EndDate.EndOfTheDay();
            persistedRetrospective.Topics = retrospective.Topics;
            persistedRetrospective.Actions = retrospective.Actions;
            
            _dbContext.SaveChanges();

            return retrospective;
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var persistedRetrospective = _dbContext.Retrospectives
                .Include(r => r.Evaluations)
                .SingleOrDefault(r => r.Id == id);

            if (persistedRetrospective == null)
            {
                return Unauthorized();
            }

            // Prevent deleting retrospective if any Evaluations have been created, we want to keep those for archival
            if (persistedRetrospective.Evaluations.Count > 0)
            {
                return BadRequest(new ErrorResponse("There are evaluations for this Retrospective. It is not possible to delete this retrospective."));
            }
            
            var teamMember = GetTeamMemberForTeam(persistedRetrospective.TeamId, User.GetId());

            if (!teamMember.Role.CanManageRetrospective)
            {
                return Unauthorized();
            }
            
            _dbContext.Retrospectives.Remove(persistedRetrospective);
            _dbContext.SaveChanges();
            
            return Ok(new SuccessResponse("Retrospective has been removed"));
        }

        [HttpGet("{id}/report")]
        public ActionResult<RetrospectiveReport> GetReport(int id)
        {
            var retrospective = _dbContext.Retrospectives
                .Include(r => r.Team)
                .Include(r => r.Topics)
                .Include(r => r.Actions)
                .SingleOrDefault(r => r.Id == id);

            if (retrospective == null) return NotFound();

            var teamMember = GetTeamMemberForTeam(retrospective.TeamId, User.GetId());
            
            if (teamMember == null)
            {
                _logger.LogWarning($"Could not find User {User.GetId()} for team {retrospective.TeamId}");
                return Unauthorized();
            }

            var evaluations = _dbContext.Evaluations
                .Include(e => e.User)
                .Where(e => e.RetrospectiveId == retrospective.Id)
                .ToList();


            var previousRetrospectiveIds = _dbContext.Retrospectives.Where(r => r.EndDate <= retrospective.EndDate && r.TeamId == retrospective.TeamId).Select(r => r.Id);

            var allSuggestedActions = _dbContext.Actions
                .Where(a => !a.IsCompleted && previousRetrospectiveIds.Contains(a.RetrospectiveId));
            
            var suggestedActions = 
                evaluations
                    .Where(e => !string.IsNullOrWhiteSpace(e.SuggestedActions))
                    .Select(e => new SuggestedAction {Description = e.SuggestedActions, SuggestedBy = e.User});
            
            var suggestedTopics = 
                evaluations
                    .Where(e => !string.IsNullOrWhiteSpace(e.SuggestedTopics))
                    .Select(e => new SuggestedTopic {Description = e.SuggestedTopics, SuggestedBy = e.User});
            
            
            var comments = _dbContext.Evaluations
                .Include(e => e.Comments)
                .ThenInclude(c => c.Evaluation)
                .ThenInclude(e => e.User)
                .Include(e => e.Comments)
                .ThenInclude(c => c.Category)
                .Where(e => e.RetrospectiveId == retrospective.Id)
                .SelectMany(e => e.Comments)
                .ToList();
            
            
            return new RetrospectiveReport
            {
                Retrospective = retrospective,
                Comments = comments.Select(c =>
                {
                    c.Evaluation = new Evaluation
                    {
                        User = c.Evaluation.User
                    };
                    return c;
                }),
                Actions = allSuggestedActions,
                SuggestedActions = suggestedActions,
                SuggestedTopics = suggestedTopics,
            };
        }

        private TeamMember GetTeamMemberForTeam(int teamId, string userId)
        {
            return _dbContext.TeamMembers.Include(tm => tm.Role)
                .SingleOrDefault(tm => tm.TeamId == teamId && tm.UserId == userId);
        }
    }
}