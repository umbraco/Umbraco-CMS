using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class TagDtoConfiguration : IEntityTypeConfiguration<TagDto>
{
    public void Configure(EntityTypeBuilder<TagDto> builder)
    {
        builder.ToTable(TagDto.TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName(TagDto.PrimaryKeyColumnName)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Group)
            .HasColumnName(TagDto.GroupColumnName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.LanguageId)
            .HasColumnName(TagDto.LanguageIdColumnName);

        builder.Property(x => x.Text)
            .HasColumnName(TagDto.TextColumnName)
            .HasMaxLength(200)
            .IsRequired();

        // IX_cmsTags (unique on group, tag, languageId)
        builder.HasIndex(x => new { x.Group, x.Text, x.LanguageId })
            .IsUnique()
            .HasDatabaseName($"IX_{TagDto.TableName}");

        // IX_cmsTags_LanguageId
        builder.HasIndex(x => x.LanguageId)
            .HasDatabaseName($"IX_{TagDto.TableName}_LanguageId");

        // IX_cmsTags_languageId_group
        // Note: SQL Server included columns (id, tag) are added by SqlServerTagDtoModelCustomizer.
        builder.HasIndex(x => new { x.LanguageId, x.Group })
            .HasDatabaseName($"IX_{TagDto.TableName}_languageId_group");

        // FK: LanguageId -> umbracoLanguage.id (optional)
        builder.HasOne<LanguageDto>()
            .WithMany()
            .HasForeignKey(x => x.LanguageId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
