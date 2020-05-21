using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduling.DataAccess.Entities;

namespace Scheduling.DataAccess.Configurations
{
    internal class RepeatIntervalConfiguration : IEntityTypeConfiguration<RepeatInterval>
    {
        public void Configure(EntityTypeBuilder<RepeatInterval> builder)
        {
            builder.ToTable("RepeatInterval", "scheduling");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedNever();

            builder.Property(e => e.Name)
                .HasMaxLength(75)
                .IsRequired();

            builder.HasData(RepeatInterval.NotUsed);
            builder.HasData(RepeatInterval.Never);
            builder.HasData(RepeatInterval.Daily);
            builder.HasData(RepeatInterval.Weekly);
            builder.HasData(RepeatInterval.Monthly);
            builder.HasData(RepeatInterval.BiMonthly);
            builder.HasData(RepeatInterval.Quarterly);
        }
    }
}