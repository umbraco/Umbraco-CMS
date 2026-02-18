using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class LastSyncedDtoConfiguration : IEntityTypeConfiguration<LastSyncedDto>
{
    public void Configure(EntityTypeBuilder<LastSyncedDto> builder)
    {
        builder.ToTable(LastSyncedDto.TableName);

        builder.HasKey(x => x.MachineId);

        builder.Property(x => x.MachineId)
            .HasColumnName("machineId");

        builder.Property(x => x.LastSyncedInternalId)
            .HasColumnName("lastSyncedInternalId");

        builder.Property(x => x.LastSyncedInternalId)
            .HasColumnName("lastSyncedInternalId");

        builder.Property(x => x.LastSyncedDate)
            .HasColumnName("lastSyncedDate");
    }
}
