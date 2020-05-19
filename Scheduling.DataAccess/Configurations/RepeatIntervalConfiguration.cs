using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduling.DataAccess.Entities;

namespace Scheduling.DataAccess.Configurations
{
    public class RepeatIntervalConfiguration : IEntityTypeConfiguration<RepeatInterval>
    {
        public void Configure(EntityTypeBuilder<RepeatInterval> builder)
        {
            builder.ToTable("RepeatInterval", "scheduling");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedNever();

            builder.Property(e => e.Name)
                .HasMaxLength(75)
                .IsRequired();
        }
    }
}