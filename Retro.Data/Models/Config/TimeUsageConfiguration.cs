using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Retro.Data.Models.Config
{
    public class TimeUsageConfiguration : IEntityTypeConfiguration<TimeUsage>
    {
        public void Configure(EntityTypeBuilder<TimeUsage> builder)
        {
            builder.HasIndex(t => new {t.EvaluationId, t.CategoryId}).IsUnique();
        }
    }
}