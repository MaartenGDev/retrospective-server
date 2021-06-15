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
    [Route("[controller]")]
    public class InsightsController : ControllerBase
    {
        private readonly RetroContext _dbContext;

        public InsightsController(RetroContext retroContext)
        {
            _dbContext = retroContext;
        }

        [HttpGet("teams/{id}/me")]
        public ActionResult<Insight> ForMe(int id)
        {
            return ForTeamMember(id, User.GetId());
        }

        [HttpGet("teams/{teamId}/members/{userId}")]
        public ActionResult<Insight> ForTeamMember(int teamId, string userId)
        {
            var authenticatedUserId = User.GetId();
            var teamMember = GetTeamMemberForTeam(teamId, authenticatedUserId);
            
            if (teamMember == null || (userId != authenticatedUserId && !teamMember.Role.CanViewMemberInsights))
            {
                return Unauthorized();
            }

            var requestedUser =
                _dbContext.TeamMembers.FirstOrDefault(tm => tm.TeamId == teamId && tm.UserId == userId);

            if (requestedUser == null)
            {
                return NotFound();
            }
            

            var metrics = GetTeamMetricsForLastRetrospective(teamId, true,$"AND UserId='{userId}'");
            var metricHistory = GetTeamMetricHistory(teamId, true, $"AND UserId='{userId}'");

            var evaluations = _dbContext.Evaluations
                .Include(e => e.Retrospective)
                .Where(e => e.UserId == userId && e.Retrospective.TeamId == teamId)
                .OrderByDescending(e => e.Retrospective.EndDate);
            
            return new Insight
            {
                Metrics = metrics,
                History = metricHistory,
                Evaluations = evaluations
            };
        }

        [HttpGet("teams/{id}/overall")]
        public ActionResult<Insight> ForTeam(int id)
        {
            var teamMember = GetTeamMemberForTeam(id, User.GetId());

            if (teamMember == null)
            {
                return Unauthorized();
            }

            var metrics = GetTeamMetricsForLastRetrospective(id, false);
            var metricHistory = GetTeamMetricHistory(id, false);

            return new Insight
            {
                Metrics = metrics,
                History = metricHistory,
                Evaluations = new List<Evaluation>()
            };
        }

        [HttpGet("teams/{id}/members")]
        public ActionResult<IEnumerable<TeamMemberInsight>> ForTeamMembers(int id)
        {
            var teamMember = GetTeamMemberForTeam(id, User.GetId());
            
            if (teamMember == null || !teamMember.Role.CanViewMemberInsights)
            {
                return Unauthorized();
            }

            var records = _dbContext.TeamMemberInsightRecords.FromSqlRaw(@"
                    SELECT TM.UserId                     as                                   UserId,
                           Users.FullName,
                           ISNULL(ROUND(ISNULL(E.SprintRating, 0) / 10, 2), 0) as                                       SprintRating,
                           ISNULL(ROUND(ISNULL((E.SprintRating - PREV_E.SprintRating) /  (SELECT MAX(Divider) FROM (VALUES (PREV_E.SprintRating),(1)) AS Calc(Divider)) * 100,
                                  0), 2), 0)                 as                                       SprintRatingChangePercentage,
                           ISNULL(TUC.Id, 0)                                                             TimeUsageCategoryId,
                           ISNULL(TUC.Name, '')                                                           TimeUsageCategoryName,
                           ISNULL(TUC.IncreaseIsPositive, 1)    as                                       TimeUsageCategoryIncreaseIsPositive,
                           ISNULL(TUC.Color, '')    as                                                    TimeUsageCategoryColor,
                           ISNULL(TU.Percentage, 0)                                           TimeUsagePercentage,

                           ISNULL(ROUND(((TU.Percentage - (SELECT CAST(PREV_TU.Percentage as float)
                                             FROM TimeUsage PREV_TU
                                             WHERE PREV_TU.EvaluationId = PREV_E.Id
                                               AND PREV_TU.CategoryId = TU.CategoryId)) /  (SELECT MAX(Divider)
                                                                                            FROM (VALUES ((SELECT PREV_TU.Percentage
                                                                                          FROM TimeUsage PREV_TU
                                                                                          WHERE PREV_TU.EvaluationId = PREV_E.Id
                                                                                            AND PREV_TU.CategoryId = TU.CategoryId)),(1)) AS Calc(Divider)) *
                                            100), 2), 0)     as                                       TimeUsageChangePercentage,

                           IIF(TUC.IncreaseIsPositive = 1, TU.Percentage, TU.Percentage * -1) SortableTimeUsagePercentage
                    FROM TeamMembers TM
                             LEFT JOIN AspNetUsers Users ON TM.UserId = Users.Id
                             LEFT JOIN Evaluations E on TM.UserId = E.UserId AND E.RetrospectiveId = (SELECT TOP 1 Id
                                                                                                      FROM Retrospectives R
                                                                                                      WHERE TeamId = {0}
                                                                                                        AND EXISTS(SELECT 1 FROM Evaluations NE WHERE NE.RetrospectiveId = R.Id AND NE.UserId=TM.UserId)
                                                                                                      ORDER BY StartDate DESC)
                             LEFT JOIN TimeUsage TU on E.Id = TU.EvaluationId
                             LEFT JOIN TimeUsageCategories TUC on TU.CategoryId = TUC.Id

                             LEFT JOIN Evaluations PREV_E on TM.UserId = PREV_E.UserId AND PREV_E.RetrospectiveId = (SELECT Id
                                                                                                                FROM Retrospectives R
                                                                                                                WHERE TeamId = {0}
                                                                                                                  AND EXISTS(SELECT 1 FROM Evaluations NE WHERE NE.RetrospectiveId = R.Id AND NE.UserId=TM.UserId)
                                                                                                                ORDER BY StartDate DESC
                                                                                                                OFFSET 1 ROWS FETCH NEXT 1 ROWS ONLY)
                    WHERE TeamId ={0} AND EXISTS(SELECT 1 FROM Evaluations E INNER JOIN Retrospectives R ON E.RetrospectiveId=R.Id WHERE R.TeamId={0} AND E.UserId=TM.UserId)
                    ORDER BY TUC.[Order] ASC, SortableTimeUsagePercentage ASC, E.SprintRating ASC", id).ToList();
            
            var insightsByUserId = new Dictionary<string, TeamMemberInsight>();
            
            foreach(var record in records)
            {
                if (!insightsByUserId.ContainsKey(record.UserId))
                {
                    insightsByUserId[record.UserId] = new TeamMemberInsight
                    {
                        UserId = record.UserId,
                        FullName = record.FullName,
                        LatestSprintRating = record.SprintRating,
                        LatestSprintRatingChangePercentage = record.SprintRatingChangePercentage
                    };
                }

                if (record.TimeUsageCategoryId != 0)
                {
                    insightsByUserId[record.UserId].TimeUsage.Add(new TeamMemberTimeUsage
                    {
                        Percentage = record.TimeUsagePercentage,
                        PercentageChangePercentage = record.TimeUsageChangePercentage,
                        Category = new TimeUsageCategory
                        {
                            Id = record.TimeUsageCategoryId,
                            Name = record.TimeUsageCategoryName,
                            Color = record.TimeUsageCategoryColor,
                            IncreaseIsPositive = record.TimeUsageCategoryIncreaseIsPositive
                        }
                    });   
                }
            }

            return insightsByUserId.Values;
        }


        [HttpGet("teams/{id}/ratings")]
        public ActionResult<IEnumerable<RatingInsight>> Ratings(int id)
        {
            var teamMember = GetTeamMemberForTeam(id, User.GetId());

            if (teamMember == null || !teamMember.Role.CanViewMemberInsights)
            {
                return Unauthorized();
            }

            return _dbContext.Evaluations
                .Include(e => e.Retrospective)
                .Include(e => e.User)
                .Where(e => e.Retrospective.TeamId == id)
                .OrderByDescending(e => e.Retrospective.EndDate)
                .Select(e => new RatingInsight
                {
                    FullName = e.User.FullName,
                    SprintRating = e.SprintRating,
                    SprintRatingExplanation = e.SprintRatingExplanation,
                    Retrospective = e.Retrospective
                })
                .ToList();
        }

        private IEnumerable<Metric> GetTeamMetricsForLastRetrospective(int teamId, bool useRetrospectiveWithCompleteEvaluations, string additionalFilter = "")
        {
            var timeUsageMetrics = _dbContext.Metrics.FromSqlRaw(
                    @"SELECT TUC.Name,
       TUC.Color,
       CONCAT(AVG(TU.Percentage), '%')                     as FormattedValue,
       ISNULL(ROUND(((AVG(TU.Percentage) - (SELECT AVG(CAST(PREV_TU.Percentage as float))
                FROM TimeUsage PREV_TU
                         LEFT JOIN TimeUsageCategories PREV_TUC on PREV_TU.CategoryId = PREV_TUC.Id
                         LEFT JOIN Evaluations PREV_E on PREV_TU.EvaluationId = PREV_E.Id
                WHERE PREV_TUC.Id = TUC.Id
                  AND PREV_E.RetrospectiveId = (SELECT Id
                                                FROM Retrospectives R
                                                WHERE TeamId = {0} AND EXISTS(SELECT 1 FROM Evaluations NE WHERE NE.RetrospectiveId=R.Id"+(useRetrospectiveWithCompleteEvaluations ? " AND NE.UserId=PREV_E.UserId" : "")+@")
                                                ORDER BY StartDate DESC
                                                OFFSET 1 ROWS FETCH NEXT 1 ROWS ONLY
                ) " + additionalFilter +
                    @")) /  (SELECT MAX(Divider)
        FROM (VALUES ((SELECT AVG(CAST(PREV_TU.Percentage as float))
                                            FROM TimeUsage PREV_TU
                                                     LEFT JOIN TimeUsageCategories PREV_TUC on PREV_TU.CategoryId = PREV_TUC.Id
                                                     LEFT JOIN Evaluations PREV_E on PREV_TU.EvaluationId = PREV_E.Id
                                            WHERE PREV_TUC.Id = TUC.Id
                                              AND PREV_E.RetrospectiveId = (SELECT Id
                                                                            FROM Retrospectives R
                                                                            WHERE TeamId = {0} AND EXISTS(SELECT 1 FROM Evaluations NE WHERE NE.RetrospectiveId=R.Id"+(useRetrospectiveWithCompleteEvaluations ? " AND NE.UserId=PREV_E.UserId" : "")+@")
                                                                            ORDER BY StartDate DESC
                                                                            OFFSET 1 ROWS FETCH NEXT 1 ROWS ONLY
                                            ) " + additionalFilter +
                    @")),(1)) AS Calc(Divider)) * 100), 2), 0) as ChangePercentage,
       TUC.IncreaseIsPositive
FROM TimeUsage TU
         LEFT JOIN TimeUsageCategories TUC on TU.CategoryId = TUC.Id
         LEFT JOIN Evaluations E on TU.EvaluationId = E.Id
WHERE RetrospectiveId = (SELECT TOP 1 Id FROM Retrospectives R WHERE TeamId = {0} AND EXISTS(SELECT 1 FROM Evaluations NE WHERE NE.RetrospectiveId=R.Id"+(useRetrospectiveWithCompleteEvaluations ? " AND NE.UserId=E.UserId" : "")+") ORDER BY StartDate DESC) " +
                    additionalFilter + @"
GROUP BY TUC.Id, TUC.Name, TUC.Color, TUC.[Order], E.RetrospectiveId, " + (additionalFilter.Length > 0 ? "E.UserId," : "") +
                    @" TUC.IncreaseIsPositive ORDER BY TUC.[Order] 
                ", teamId)
                .ToList();

            var generalMetrics = _dbContext.Metrics.FromSqlRaw(
                    @"
                        SELECT 'Sprint Rating'                      as Name,
                               '#3B4558'                            as Color,
                               CONCAT(ROUND(AVG(E.SprintRating) / 10, 2), '') as FormattedValue,
                               ISNULL(ROUND((((AVG(E.SprintRating) - (SELECT AVG(PREV_E.SprintRating)
                                                                      FROM Evaluations PREV_E
                                                                      WHERE PREV_E.RetrospectiveId = (SELECT Id
                                                                                                      FROM Retrospectives R
                                                                                                      WHERE TeamId = {0}
                                                                                                        AND EXISTS(SELECT 1 FROM Evaluations NE WHERE NE.RetrospectiveId = R.Id"+ (useRetrospectiveWithCompleteEvaluations ? " AND NE.UserId=PREV_E.UserId" : "")+@")
                                                                                                      ORDER BY StartDate DESC
                                                                                                      OFFSET 1 ROWS FETCH NEXT 1 ROWS ONLY
                                                                      ) " + additionalFilter + @"
                               )) /    (SELECT MAX(Divider)
        FROM (VALUES ((SELECT AVG(PREV_E.SprintRating)
                                     FROM Evaluations PREV_E
                                     WHERE PREV_E.RetrospectiveId = (SELECT Id
                                                                     FROM Retrospectives R
                                                                     WHERE TeamId = {0}
                                                                       AND EXISTS(SELECT 1 FROM Evaluations NE WHERE NE.RetrospectiveId = R.Id"+(useRetrospectiveWithCompleteEvaluations ? " AND NE.UserId=PREV_E.UserId" : "")+@")
                                                                     ORDER BY StartDate DESC
                                                                     OFFSET 1 ROWS FETCH NEXT 1 ROWS ONLY
                                     ) " + additionalFilter + @")   ),(1)) AS Calc(Divider)))) * 100, 2), 0)             as ChangePercentage,
                               CAST(1 as bit)                       as IncreaseIsPositive
                        FROM Evaluations E
                        WHERE RetrospectiveId = (SELECT TOP 1 Id
                                                 FROM Retrospectives R
                                                 WHERE TeamId = {0}
                                                   AND EXISTS(SELECT 1 FROM Evaluations NE WHERE NE.RetrospectiveId = R.Id" + (useRetrospectiveWithCompleteEvaluations ? " AND NE.UserId=E.UserId" : "")+@")
                                                 ORDER BY StartDate DESC) " + additionalFilter + @"
                    ", teamId)
                .ToList();

            return timeUsageMetrics.Concat(generalMetrics);
        }

        private MetricHistory GetTeamMetricHistory(int teamId, bool useRetrospectiveWithCompleteEvaluations, string additionalFilter = "")
        {
            var teamMetricHistoryForTimeUsage = GetTeamMetricHistoryForTimeUsage(teamId, useRetrospectiveWithCompleteEvaluations, additionalFilter);

            var sprintRatingHistoryRecords = _dbContext.MetricHistoryRecords.FromSqlRaw(@"
                SELECT '' as Name, '' as Color, 0 as CategoryId, ROUND(AVG(E.SprintRating), 2) as Value, E.RetrospectiveId
                FROM Evaluations E
                LEFT JOIN Retrospectives R2 on E.RetrospectiveId = R2.Id
                WHERE E.RetrospectiveId IN (SELECT Id FROM Retrospectives R WHERE TeamId = {0} AND EXISTS(SELECT 1 FROM Evaluations NE WHERE NE.RetrospectiveId=R.Id)) " +
                                                                                        additionalFilter + @"
                GROUP BY E.RetrospectiveId, R2.StartDate
                ORDER BY R2.StartDate ASC
            ", teamId).ToList().Select(m => m.Value);

            var sprintRatingDataset = new Dataset()
            {
                Label = "Sprint Rating",
                Data = sprintRatingHistoryRecords,
                Color = "#3B4558"
            };

            return new MetricHistory()
            {
                Datasets = teamMetricHistoryForTimeUsage.Datasets.Concat(new List<Dataset> {sprintRatingDataset}),
                Labels = teamMetricHistoryForTimeUsage.Labels
            };
        }

        private MetricHistory GetTeamMetricHistoryForTimeUsage(int teamId, bool useRetrospectiveWithCompleteEvaluations, string additionalFilter = "")
        {
            var historyRecords = _dbContext.MetricHistoryRecords.FromSqlRaw(@"
                SELECT TUC.Name, TUC.Color, TU.CategoryId, CAST(AVG(TU.Percentage) as double precision) as Value, E.RetrospectiveId
                FROM TimeUsage TU
                         LEFT JOIN TimeUsageCategories TUC on TU.CategoryId = TUC.Id
                         LEFT JOIN Evaluations E on TU.EvaluationId = E.Id
                         LEFT JOIN Retrospectives R ON R.Id=E.RetrospectiveId
                WHERE RetrospectiveId IN
                      (SELECT Id FROM Retrospectives R WHERE TeamId={0} AND EXISTS(SELECT 1 FROM Evaluations NE WHERE NE.RetrospectiveId=R.Id"+ (useRetrospectiveWithCompleteEvaluations ? " AND NE.UserId=E.UserId" : "")+")) " +
                                                                            additionalFilter + @"
                GROUP BY TU.CategoryId, TUC.Name, Tuc.Color, E.RetrospectiveId,R.StartDate, TUC.IncreaseIsPositive
                ORDER BY R.StartDate ASC
            ", teamId);

            var categoryNamesById = new Dictionary<int, string>();
            var colorsByCategory = new Dictionary<int, string>();
            var valuesByCategory = new Dictionary<int, List<double>>();
            var ids = new List<int>();

            foreach (var historyRecord in historyRecords)
            {
                if (!ids.Contains(historyRecord.RetrospectiveId))
                {
                    ids.Add(historyRecord.RetrospectiveId);
                }

                if (!valuesByCategory.ContainsKey(historyRecord.CategoryId))
                {
                    colorsByCategory[historyRecord.CategoryId] = historyRecord.Color;
                    categoryNamesById[historyRecord.CategoryId] = historyRecord.Name;
                    valuesByCategory[historyRecord.CategoryId] = new List<double>();
                }

                valuesByCategory[historyRecord.CategoryId].Add(historyRecord.Value);
            }

            var datasets = valuesByCategory.Keys.Select(categoryId =>
            {
                return new Dataset()
                {
                    Label = categoryNamesById[categoryId],
                    Data = valuesByCategory[categoryId],
                    Color = colorsByCategory[categoryId]
                };
            });

            var retrospectives = _dbContext.Retrospectives.Where(r => ids.Contains(r.Id))
                .ToList()
                .ToDictionary(r => r.Id);

            var labels = valuesByCategory.Keys.Count == 0
                ? new List<string>()
                : ids.Select((id, index) => retrospectives[id].Name);


            return new MetricHistory
            {
                Datasets = datasets,
                Labels = labels
            };
        }
        
        private TeamMember GetTeamMemberForTeam(int teamId, string userId)
        {
            return _dbContext.TeamMembers.Include(tm => tm.Role)
                .SingleOrDefault(tm => tm.TeamId == teamId && tm.UserId == userId);
        }
    }
}