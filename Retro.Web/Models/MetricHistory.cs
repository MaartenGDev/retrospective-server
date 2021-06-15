using System.Collections.Generic;
using Retro.Data.Models;

namespace Retro.Web.Models
{
    public class MetricHistory
    {
        public IEnumerable<Dataset> Datasets { get; set; }
        public IEnumerable<string> Labels { get; set; }
    }
}