using System.Collections.Generic;

namespace Retro.Web.Models
{
    public class Dataset
    {
        public string Label { get; set; }
        public IEnumerable<double> Data { get; set; }
        
        public string Color { get; set; }
    }
}