using Retro.Data.Models;

namespace Retro.Web.Models
{
    public class SuggestedTopic
    {
        public string Description { get; set; }
        public ApplicationUser SuggestedBy { get; set; }
    }
}