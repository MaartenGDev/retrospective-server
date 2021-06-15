using System.ComponentModel.DataAnnotations.Schema;

namespace Retro.Data.Models
{
    public class Metric
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public string FormattedValue { get; set; }
        public double ChangePercentage { get; set; }
        public bool IncreaseIsPositive { get; set; }
    }
}