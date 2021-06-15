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

namespace Retro.Web.Controllers
{
    [Authorize]
    [ApiController]
    public class EvaluationsController : ControllerBase {
        private readonly RetroContext _dbContext;

        public EvaluationsController(RetroContext retroContext)
        {
            _dbContext = retroContext;
        }

        [HttpPatch("retrospectives/{id}/evaluation")]
        public ActionResult<Evaluation> Update(int id, [FromBody] Evaluation evaluation)
        {
            var authenticatedUserId = User.GetId();
            var retrospective = _dbContext.Retrospectives.Single(r => r.Id == id);

            if (!IsValidEvaluation(evaluation))
            {
                return BadRequest(new ErrorResponse("Failed to save feedback, the provided feedback is invalid!"));
            }
            
            var evaluationToPersist = _dbContext.Evaluations
                .Include(e => e.Comments)
                .Include(e => e.Retrospective)
                .SingleOrDefault(e => e.RetrospectiveId == id && e.UserId == authenticatedUserId);

            var isNewEvaluation = evaluationToPersist == null;
            
            if (isNewEvaluation)
            {
                evaluationToPersist = new Evaluation
                {
                    Retrospective = retrospective,
                    UserId = authenticatedUserId
                };
            }

            if (DateTime.Now > evaluationToPersist.Retrospective.EndDate)
            {
                return BadRequest(new ErrorResponse($"The feedback period for this retrospective has already ended at {evaluationToPersist.Retrospective.EndDate}"));
            }

            var latestCommentIds = evaluation.Comments.Select(c => c.Id);
            var commentsToDelete = evaluationToPersist.Comments.Where(c => !latestCommentIds.Contains(c.Id));

            _dbContext.Comments.RemoveRange(commentsToDelete);
            
            evaluationToPersist.SprintRating = evaluation.SprintRating;
            evaluationToPersist.SprintRatingExplanation = evaluation.SprintRatingExplanation;
            evaluationToPersist.SuggestedActions = evaluation.SuggestedActions;
            evaluationToPersist.SuggestedTopics = evaluation.SuggestedTopics;
            evaluationToPersist.Comments = evaluation.Comments;
            evaluationToPersist.TimeUsage = evaluation.TimeUsage;

            if (isNewEvaluation)
            {
                _dbContext.Evaluations.Add(evaluationToPersist);
            }
            

            _dbContext.SaveChanges();

            return evaluationToPersist;
        }

        private bool IsValidEvaluation(Evaluation evaluation)
        {
            var totalTimeUsagePercentage = evaluation.TimeUsage.Sum(t => t.Percentage);

            if (totalTimeUsagePercentage != 100)
            {
                return false;
            }

            return true;
        }
    }
}