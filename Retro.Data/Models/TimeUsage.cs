using System.ComponentModel.DataAnnotations;

namespace Retro.Data.Models
{
    public class TimeUsage
    {
        public int Id { get; set; }
        [Required]
        public int Percentage { get; set; }
        
        [Required]
        public int EvaluationId { get; set; }
        public Evaluation Evaluation { get; set; }
        
        [Required]
        public int CategoryId { get; set; }
        public TimeUsageCategory Category { get; set; }
    }
}