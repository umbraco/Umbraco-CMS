using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class ContentVersionCultureVariationDtoConfiguration : IEntityTypeConfiguration<ContentVersionCultureVariationDto>
{
    public void Configure(EntityTypeBuilder<ContentVersionCultureVariationDto> builder)
    {
        builder.ToTable(ContentVersionCultureVariationDto.TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName(ContentVersionCultureVariationDto.PrimaryKeyColumnName)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.VersionId)
            .HasColumnName(ContentVersionCultureVariationDto.VersionIdColumnName);

        builder.Property(x => x.LanguageId)
            .HasColumnName(ContentVersionCultureVariationDto.LanguageIdColumnName);

        builder.Property(x => x.Name)
            .HasColumnName(ContentVersionCultureVariationDto.NameColumnName);

        builder.Property(x => x.UpdateDate)
            .HasColumnName(ContentVersionCultureVariationDto.UpdateDateColumnName);

        builder.Property(x => x.UpdateUserId)
            .HasColumnName(ContentVersionCultureVariationDto.UpdateUserIdColumnName);

        // FK: VersionId -> umbracoContentVersion.id
        builder.HasOne<ContentVersionDto>()
            .WithMany()
            .HasForeignKey(x => x.VersionId)
            .OnDelete(DeleteBehavior.Cascade);

        // FK: LanguageId -> umbracoLanguage.id
        builder.HasOne<LanguageDto>()
            .WithMany()
            .HasForeignKey(x => x.LanguageId)
            .OnDelete(DeleteBehavior.Cascade);

        // IX_umbracoContentVersionCultureVariation_VersionId (unique, composite on VersionId+LanguageId)
        // Note: SQL Server included columns are added by SqlServerContentVersionCultureVariationDtoModelCustomizer.
        builder.HasIndex(x => new { x.VersionId, x.LanguageId })
            .IsUnique()
            .HasDatabaseName($"IX_{ContentVersionCultureVariationDto.TableName}_VersionId");

        // IX_umbracoContentVersionCultureVariation_LanguageId
        builder.HasIndex(x => x.LanguageId)
            .HasDatabaseName($"IX_{ContentVersionCultureVariationDto.TableName}_LanguageId");
    }
}
