using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Retro.Data.Models
{
    public class Evaluation
    {
        public int Id { get; set; }
        [Required]
        public double SprintRating { get; set; }
        [Required(AllowEmptyStrings = true)]
        public string SprintRatingExplanation { get; set; }
        public string SuggestedActions { get; set; } 
        public string SuggestedTopics { get; set; }

        public List<Comment> Comments { get; set; } = new List<Comment>();
        public List<TimeUsage> TimeUsage { get; set; } = new List<TimeUsage>();
        
        [Required]
        public int RetrospectiveId { get; set; }
        public Retrospective Retrospective { get; set; }
        
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}