using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class LongRunningOperationDtoConfiguration : IEntityTypeConfiguration<LongRunningOperationDto>
{
    public void Configure(EntityTypeBuilder<LongRunningOperationDto> builder)
    {
        builder.ToTable(LongRunningOperationDto.TableName);

        builder.HasKey(x => x.Id)
            .HasName($"PK_{LongRunningOperationDto.TableName}");

        builder.Property(x => x.Id)
            .HasColumnName(LongRunningOperationDto.IdColumnName)
            .ValueGeneratedNever();

        builder.Property(x => x.Type)
            .HasColumnName(LongRunningOperationDto.TypeColumnName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName(LongRunningOperationDto.StatusColumnName)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Result)
            .HasColumnName(LongRunningOperationDto.ResultColumnName);

        builder.Property(x => x.CreateDate)
            .HasColumnName(LongRunningOperationDto.CreateDateColumnName);

        builder.Property(x => x.UpdateDate)
            .HasColumnName(LongRunningOperationDto.UpdateDateColumnName);

        builder.Property(x => x.ExpirationDate)
            .HasColumnName(LongRunningOperationDto.ExpirationDateColumnName);
    }
}
