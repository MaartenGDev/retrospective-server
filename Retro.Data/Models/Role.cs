using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Retro.Data.Models
{
    public class Role
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public bool CanManageTeam { get; set; }
        public bool CanManageRetrospective { get; set; }
        public bool CanViewMemberInsights { get; set; }
    }
}