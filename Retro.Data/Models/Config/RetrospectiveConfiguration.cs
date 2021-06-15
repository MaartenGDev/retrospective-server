using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Retro.Data.Models.Config
{
    public class TeamConfiguration : IEntityTypeConfiguration<Retrospective>
    {
        public void Configure(EntityTypeBuilder<Retrospective> builder)
        {
            builder.HasOne(r => r.Team)
                .WithMany(t => t.Retrospectives)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}