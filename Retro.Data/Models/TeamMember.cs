using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Retro.Data.Models
{
    public class TeamMember
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        [Required]
        public int TeamId { get; set; }
        public Team Team { get; set; }
        
        public int RoleId { get; set; }
        public Role Role { get; set; }
    }
}