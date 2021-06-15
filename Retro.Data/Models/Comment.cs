using System.ComponentModel.DataAnnotations;

namespace Retro.Data.Models
{
    public class Comment
    {
        public int Id { get; set; }
        [Required]
        public string Body { get; set; }

        [Required]
        public int CategoryId { get; set; }
        public CommentCategory Category { get; set; }
        
        [Required]
        public int EvaluationId { get; set; }
        public Evaluation Evaluation { get; set; }
    }
}