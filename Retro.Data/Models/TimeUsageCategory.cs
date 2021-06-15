using System.ComponentModel.DataAnnotations;

namespace Retro.Data.Models
{
    public class TimeUsageCategory
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Color { get; set; }
        [Required]
        public int InitialPercentage { get; set; }
        
        public bool IncreaseIsPositive { get; set; }
        
        [Required]
        public int Order { get; set; }
    }
}