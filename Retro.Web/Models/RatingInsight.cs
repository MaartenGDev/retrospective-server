using System.Collections.Generic;
using Retro.Data.Models;

namespace Retro.Web.Models
{
    public class RatingInsight
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public double SprintRating { get; set; }
        public string SprintRatingExplanation { get; set; }
        public Retrospective Retrospective { get; set; }
    }
}