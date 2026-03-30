using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class LanguageTextDtoConfiguration : IEntityTypeConfiguration<LanguageTextDto>
{
    public void Configure(EntityTypeBuilder<LanguageTextDto> builder)
    {
        builder.ToTable(LanguageTextDto.TableName);

        builder.HasKey(x => x.PrimaryKey);

        builder.Property(x => x.PrimaryKey)
            .HasColumnName(LanguageTextDto.PrimaryKeyColumnName)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.LanguageId)
            .HasColumnName("languageId");

        builder.Property(x => x.UniqueId)
            .HasColumnName("UniqueId");

        builder.Property(x => x.Value)
            .HasColumnName("value")
            .HasMaxLength(1000);

        // Composite unique index on (LanguageId, UniqueId)
        builder.HasIndex(x => new { x.LanguageId, x.UniqueId })
            .IsUnique()
            .HasDatabaseName($"IX_{LanguageTextDto.TableName}_languageId");

        // FK to Language table
        builder.HasOne(x => x.Language)
            .WithMany()
            .HasForeignKey(x => x.LanguageId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
