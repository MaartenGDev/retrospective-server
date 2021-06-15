using System.Collections.Generic;
using Retro.Data.Models;

namespace Retro.Web.Models
{
    public class RetrospectiveReport
    {
        public Retrospective Retrospective { get; set; }
        public IEnumerable<Comment> Comments { get; set; }
        public IEnumerable<SuggestedTopic> SuggestedTopics { get; set; }
        public IEnumerable<SuggestedAction> SuggestedActions { get; set; }
        public IEnumerable<Action> Actions { get; set; }
    }
}