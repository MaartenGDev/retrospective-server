using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Retro.Data.Models.Config
{
    public class MetricHistoryRowConfiguration : IEntityTypeConfiguration<MetricHistoryRecord>
    {
        public void Configure(EntityTypeBuilder<MetricHistoryRecord> builder)
        {
            builder.HasNoKey().ToView(null);
        }
    }
}