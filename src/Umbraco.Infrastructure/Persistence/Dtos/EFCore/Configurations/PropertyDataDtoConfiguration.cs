using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class PropertyDataDtoConfiguration : IEntityTypeConfiguration<PropertyDataDto>
{
    public void Configure(EntityTypeBuilder<PropertyDataDto> builder)
    {
        builder.ToTable(PropertyDataDto.TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName(PropertyDataDto.PrimaryKeyColumnName)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.VersionId)
            .HasColumnName(PropertyDataDto.VersionIdColumnName);

        builder.Property(x => x.PropertyTypeId)
            .HasColumnName(PropertyDataDto.PropertyTypeIdColumnName);

        builder.Property(x => x.LanguageId)
            .HasColumnName(PropertyDataDto.LanguageIdColumnName);

        builder.Property(x => x.Segment)
            .HasColumnName(PropertyDataDto.SegmentColumnName)
            .HasMaxLength(PropertyDataDto.SegmentLength);

        builder.Property(x => x.IntegerValue)
            .HasColumnName("intValue");

        builder.Property(x => x.DecimalValue)
            .HasColumnName("decimalValue");

        builder.Property(x => x.DateValue)
            .HasColumnName("dateValue");

        builder.Property(x => x.VarcharValue)
            .HasColumnName("varcharValue")
            .HasMaxLength(PropertyDataDto.VarcharLength);

        builder.Property(x => x.TextValue)
            .HasColumnName("textValue");

        builder.Property(x => x.SortableValue)
            .HasColumnName("sortableValue")
            .HasMaxLength(PropertyDataDto.VarcharLength);

        // FK: VersionId -> umbracoContentVersion.id
        builder.HasOne<ContentVersionDto>()
            .WithMany()
            .HasForeignKey(x => x.VersionId)
            .OnDelete(DeleteBehavior.Cascade);

        // FK: LanguageId -> umbracoLanguage.id
        builder.HasOne<LanguageDto>()
            .WithMany()
            .HasForeignKey(x => x.LanguageId)
            .OnDelete(DeleteBehavior.NoAction);

        // IX_umbracoPropertyData_VersionId (unique, composite on VersionId+PropertyTypeId+LanguageId+Segment)
        builder.HasIndex(x => new { x.VersionId, x.PropertyTypeId, x.LanguageId, x.Segment })
            .IsUnique()
            .HasDatabaseName($"IX_{PropertyDataDto.TableName}_VersionId");

        // IX_umbracoPropertyData_PropertyTypeId
        builder.HasIndex(x => x.PropertyTypeId)
            .HasDatabaseName($"IX_{PropertyDataDto.TableName}_PropertyTypeId");

        // IX_umbracoPropertyData_LanguageId
        builder.HasIndex(x => x.LanguageId)
            .HasDatabaseName($"IX_{PropertyDataDto.TableName}_LanguageId");

        // IX_umbracoPropertyData_Segment
        builder.HasIndex(x => x.Segment)
            .HasDatabaseName($"IX_{PropertyDataDto.TableName}_Segment");
    }
}
