using System.Collections.Generic;
using Retro.Data.Models;

namespace Retro.Web.Models
{
    public class TeamMemberInsight
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public double LatestSprintRating { get; set; }
        public double LatestSprintRatingChangePercentage { get; set; }
        public List<TeamMemberTimeUsage> TimeUsage { get; } = new List<TeamMemberTimeUsage>();
    }
}