using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class DistributedJobConfiguration : IEntityTypeConfiguration<DistributedJobDto>
{
    public void Configure(EntityTypeBuilder<DistributedJobDto> builder)
    {
        builder.ToTable(DistributedJobDto.TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName(DistributedJobDto.PrimaryKeyColumnName)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .HasColumnName("Name")
            .IsRequired();

        builder.Property(x => x.LastRun)
            .HasColumnName("lastRun");

        builder.Property(x => x.Period)
            .HasColumnName("period");

        builder.Property(x => x.IsRunning)
            .HasColumnName("IsRunning");

        builder.Property(x => x.LastAttemptedRun)
            .HasColumnName("lastAttemptedRun");
    }
}
