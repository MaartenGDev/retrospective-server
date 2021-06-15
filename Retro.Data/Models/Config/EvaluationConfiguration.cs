using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Retro.Data.Models.Config
{
    public class EvaluationConfiguration : IEntityTypeConfiguration<Evaluation>
    {
        public void Configure(EntityTypeBuilder<Evaluation> builder)
        {
            builder.HasIndex(e => new {e.RetrospectiveId, e.UserId}).IsUnique();
            
            builder.HasOne(e => e.Retrospective)
                .WithMany(t => t.Evaluations)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}