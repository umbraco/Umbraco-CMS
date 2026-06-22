using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class PropertyTypeDtoConfiguration : IEntityTypeConfiguration<PropertyTypeDto>
{
    public void Configure(EntityTypeBuilder<PropertyTypeDto> builder)
    {
        builder.ToTable(PropertyTypeDto.TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName(PropertyTypeDto.PrimaryKeyColumnName)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.DataTypeId)
            .HasColumnName(PropertyTypeDto.DataTypeIdColumnName);

        builder.Property(x => x.ContentTypeId)
            .HasColumnName(PropertyTypeDto.ContentTypeIdColumnName);

        builder.Property(x => x.PropertyTypeGroupId)
            .HasColumnName(PropertyTypeDto.PropertyTypeGroupIdColumnName);

        builder.Property(x => x.Alias)
            .HasColumnName(PropertyTypeDto.AliasColumnName);

        builder.Property(x => x.Name)
            .HasColumnName(PropertyTypeDto.NameColumnName);

        builder.Property(x => x.SortOrder)
            .HasColumnName(PropertyTypeDto.SortOrderColumnName)
            .HasDefaultValue(0);

        builder.Property(x => x.Mandatory)
            .HasColumnName(PropertyTypeDto.MandatoryColumnName)
            .HasDefaultValue(false);

        builder.Property(x => x.MandatoryMessage)
            .HasColumnName(PropertyTypeDto.MandatoryMessageColumnName)
            .HasMaxLength(500);

        builder.Property(x => x.ValidationRegExp)
            .HasColumnName(PropertyTypeDto.ValidationRegExpColumnName);

        builder.Property(x => x.ValidationRegExpMessage)
            .HasColumnName(PropertyTypeDto.ValidationRegExpMessageColumnName)
            .HasMaxLength(500);

        builder.Property(x => x.Description)
            .HasColumnName(PropertyTypeDto.DescriptionColumnName)
            .HasMaxLength(2000);

        builder.Property(x => x.LabelOnTop)
            .HasColumnName(PropertyTypeDto.LabelOnTopColumnName)
            .HasDefaultValue(false);

        builder.Property(x => x.Variations)
            .HasColumnName(PropertyTypeDto.VariationsColumnName);

        builder.Property(x => x.UniqueId)
            .HasColumnName(PropertyTypeDto.UniqueIdColumnName);

        // IX_cmsPropertyTypeAlias (non-unique on Alias)
        builder.HasIndex(x => x.Alias)
            .HasDatabaseName("IX_cmsPropertyTypeAlias");

        // IX_cmsPropertyTypeUniqueID (unique on UniqueId)
        builder.HasIndex(x => x.UniqueId)
            .IsUnique()
            .HasDatabaseName("IX_cmsPropertyTypeUniqueID");

        // FK to cmsDataType (DataTypeId -> DataTypeDto.NodeId, which is its primary key)
        builder.HasOne(x => x.DataTypeDto)
            .WithMany()
            .HasForeignKey(x => x.DataTypeId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName($"FK_{PropertyTypeDto.TableName}_{DataTypeDto.TableName}");

        // FK to cmsPropertyTypeGroup (PropertyTypeGroupId -> PropertyTypeGroupDto.Id)
        builder.HasOne(x => x.PropertyTypeGroupDto)
            .WithMany(x => x.PropertyTypeDtos)
            .HasForeignKey(x => x.PropertyTypeGroupId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName($"FK_{PropertyTypeDto.TableName}_{PropertyTypeGroupDto.TableName}");

        // No EF Core navigation is declared for the content-type FK: it references the alternate key
        // (nodeId) rather than its primary key.
        // TODO (EF Core): the FK to cmsContentType.nodeId is currently created by NPoco's schema; revisit this comment once NPoco is removed.
    }
}
