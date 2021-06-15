using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Retro.Data.Models
{
    public class Retrospective
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public List<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
        public List<Topic> Topics { get; set; } = new List<Topic>();
        public List<Action> Actions { get; set; } = new List<Action>();
        
        public int TeamId { get; set; }
        public Team Team { get; set; }
    }
}