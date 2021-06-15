using System.Collections.Generic;
using Retro.Data.Models;

namespace Retro.Web.Models
{
    public class Insight
    {
        public IEnumerable<Metric> Metrics { get; set; }
        public MetricHistory History { get; set; }
        public IEnumerable<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
    }
}