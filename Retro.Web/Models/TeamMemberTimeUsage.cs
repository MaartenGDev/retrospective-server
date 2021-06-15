using Retro.Data.Models;

namespace Retro.Web.Models
{
    public class TeamMemberTimeUsage
    {
        public int Percentage { get; set; }
        public double PercentageChangePercentage { get; set; }
        public TimeUsageCategory Category { get; set; }
    }
}