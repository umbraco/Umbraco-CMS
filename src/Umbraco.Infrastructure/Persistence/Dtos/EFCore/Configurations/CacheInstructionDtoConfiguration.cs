using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class CacheInstructionDtoConfiguration : IEntityTypeConfiguration<CacheInstructionDto>
{
    public void Configure(EntityTypeBuilder<CacheInstructionDto> builder)
    {
        builder.ToTable(CacheInstructionDto.TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UtcStamp)
            .HasColumnName("utcStamp");

        builder.Property(x => x.Instructions)
            .HasColumnName("jsonInstruction")
            .HasMaxLength(int.MaxValue);

        builder.Property(x => x.OriginIdentity)
            .HasColumnName("originated")
            .HasMaxLength(500);

        builder.Property(x => x.InstructionCount)
            .HasColumnName("instructionCount")
            .HasDefaultValue(1);
    }
}
