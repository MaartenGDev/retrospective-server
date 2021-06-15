using System.ComponentModel.DataAnnotations;

namespace Retro.Data.Models
{
    public class Topic
    {
        public int Id { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public int DurationInMinutes { get; set; }
        
        [Required]
        public int RetrospectiveId { get; set; }
        
        [Required]
        public int Order { get; set; }
        
        // is required but not marked as such to skip validation
        public Retrospective Retrospective { get; set; }
    }
}