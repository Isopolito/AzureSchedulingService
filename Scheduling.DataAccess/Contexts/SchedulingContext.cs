using Microsoft.EntityFrameworkCore;
using Scheduling.DataAccess.Configurations;
using Scheduling.DataAccess.Entities;

namespace Scheduling.DataAccess.Contexts
{
    public class SchedulingContext : DbContext
    {
        public SchedulingContext(DbContextOptions<SchedulingContext> options) : base(options)
        {
        }

        public DbSet<Job> Jobs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new JobConfiguration());
            modelBuilder.ApplyConfiguration(new RepeatEndStrategyConfiguration());
            modelBuilder.ApplyConfiguration(new RepeatIntervalConfiguration());
        }
    }
 }