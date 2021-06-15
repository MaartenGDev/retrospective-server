using Retro.Data.Models;

namespace Retro.Web.Models
{
    public class SuggestedAction
    {
        public string Description { get; set; }
        public ApplicationUser SuggestedBy { get; set; }
    }
}