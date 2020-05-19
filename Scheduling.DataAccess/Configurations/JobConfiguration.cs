using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduling.DataAccess.Entities;

namespace Scheduling.DataAccess.Configurations
{
    public class JobConfiguration : IEntityTypeConfiguration<Job>
    {
        public void Configure(EntityTypeBuilder<Job> builder)
        {
            builder.ToTable("Job", "scheduling");

            builder.HasKey(e => e.JobId);

            builder.Property(e => e.JobIdentifier)
                .HasMaxLength(75)
                .IsRequired();

            builder.Property(e => e.SubscriptionName)
                .HasMaxLength(75)
                .IsRequired();

            builder.HasIndex(e => new { e.SubscriptionName, e.JobIdentifier })
                .IsUnique()
                .HasName("UX_scheduling_Job_JobIdentifier_SubscriptionName");

            builder.Property(e => e.CronExpressionOverride)
                .HasMaxLength(998);

            builder.Property(e => e.DomainName)
                .HasMaxLength(2048);

            builder.Property(e => e.StartAt)
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .IsRequired();

            builder.Property(e => e.UpdatedAt)
                .IsRequired();
           
            builder.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(e => e.UpdatedBy)
                .IsRequired()
                .HasMaxLength(128);
        }
    }
}