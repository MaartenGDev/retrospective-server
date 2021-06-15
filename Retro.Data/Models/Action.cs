using System.ComponentModel.DataAnnotations;

namespace Retro.Data.Models
{
    public class Action
    {
        public int Id { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string Responsible { get; set; }
        [Required]
        public bool IsCompleted { get; set; }
        
        [Required]
        public int RetrospectiveId { get; set; }
        public Retrospective Retrospective { get; set; }
    }
}