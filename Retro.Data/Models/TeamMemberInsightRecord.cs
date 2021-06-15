namespace Retro.Data.Models
{
    public class TeamMemberInsightRecord
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public double SprintRating { get; set; }
        public double SprintRatingChangePercentage { get; set; }
        public int TimeUsageCategoryId { get; set; }
        public string TimeUsageCategoryName { get; set; }
        public bool TimeUsageCategoryIncreaseIsPositive { get; set; }
        public string TimeUsageCategoryColor { get; set; }
        public int TimeUsagePercentage { get; set; }
        public double TimeUsageChangePercentage { get; set; }
    }
}