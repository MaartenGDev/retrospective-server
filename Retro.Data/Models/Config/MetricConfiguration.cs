using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Retro.Data.Models.Config
{
    public class MetricConfiguration : IEntityTypeConfiguration<Metric>
    {
        public void Configure(EntityTypeBuilder<Metric> builder)
        {
            builder.HasNoKey().ToView(null);
        }
    }
}