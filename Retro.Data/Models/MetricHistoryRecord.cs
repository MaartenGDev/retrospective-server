namespace Retro.Data.Models
{
    public class MetricHistoryRecord
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public int CategoryId { get; set; }
        public double Value { get; set; }
        public int RetrospectiveId { get; set; }
    }
}