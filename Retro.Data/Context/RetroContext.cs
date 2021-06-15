using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Retro.Data.Models;

namespace Retro.Data.Context
{
    public class RetroContext : IdentityDbContext<ApplicationUser>
    {
        public RetroContext (DbContextOptions<RetroContext> options) : base(options)
        {}

        public DbSet<Retrospective> Retrospectives { get; set; }
        public DbSet<Evaluation> Evaluations { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Action> Actions { get; set; }
        public DbSet<CommentCategory> CommentCategories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        
        public DbSet<TimeUsageCategory> TimeUsageCategories { get; set; }
        public DbSet<Metric> Metrics { get; set; }
        public DbSet<MetricHistoryRecord> MetricHistoryRecords { get; set; }
        public DbSet<TeamMemberInsightRecord> TeamMemberInsightRecords { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<Role> TeamMemberRoles { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(RetroContext).Assembly);
            
            base.OnModelCreating(modelBuilder);
        }
    }
}