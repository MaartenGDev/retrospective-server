using System.ComponentModel.DataAnnotations;

namespace Retro.Data.Models
{
    public class CommentCategory
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string IconLabel { get; set; }
        [Required]
        public string IconColor { get; set; }
        [Required]
        public int MinimalCommentCount { get; set; }
    }
}