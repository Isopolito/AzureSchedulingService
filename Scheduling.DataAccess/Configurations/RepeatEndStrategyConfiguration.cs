using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduling.DataAccess.Entities;

namespace Scheduling.DataAccess.Configurations
{
    internal class RepeatEndStrategyConfiguration : IEntityTypeConfiguration<RepeatEndStrategy>
    {
        public void Configure(EntityTypeBuilder<RepeatEndStrategy> builder)
        {
            builder.ToTable("RepeatEndStrategy", "scheduling");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedNever();

            builder.Property(e => e.Name)
                .HasMaxLength(75)
                .IsRequired();

            builder.HasData(RepeatEndStrategy.NotUsed);
            builder.HasData(RepeatEndStrategy.Never);
            builder.HasData(RepeatEndStrategy.AfterOccurrenceNumber);
            builder.HasData(RepeatEndStrategy.OnEndDate);
        }
    }
}