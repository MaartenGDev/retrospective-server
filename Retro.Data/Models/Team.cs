using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Retro.Data.Models
{
    public class Team
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string InviteCode { get; set; }
        
        public List<TeamMember> Members { get; set; } = new List<TeamMember>();
        public List<Retrospective> Retrospectives { get; set; } = new List<Retrospective>();
    }
}