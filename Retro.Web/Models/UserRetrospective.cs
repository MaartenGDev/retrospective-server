using System;
using System.Collections.Generic;
using Retro.Data.Models;
using Action = Retro.Data.Models.Action;

namespace Retro.Web.Models
{
    public class UserRetrospective
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public Evaluation Evaluation { get; set; }
        
        public List<Topic> Topics { get; set; } = new List<Topic>();
        public List<Action> Actions { get; set; } = new List<Action>();
        
        public Team Team { get; set; }
    }
}