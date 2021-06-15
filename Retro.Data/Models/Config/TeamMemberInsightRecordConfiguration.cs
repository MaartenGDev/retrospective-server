using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Retro.Data.Models.Config
{
    public class TeamMemberInsightRecordConfiguration : IEntityTypeConfiguration<TeamMemberInsightRecord>
    {
        public void Configure(EntityTypeBuilder<TeamMemberInsightRecord> builder)
        {
            builder.HasNoKey().ToView(null);
        }
    }
}