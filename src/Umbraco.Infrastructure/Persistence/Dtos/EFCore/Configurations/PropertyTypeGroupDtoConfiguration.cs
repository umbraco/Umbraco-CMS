using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class PropertyTypeGroupDtoConfiguration : IEntityTypeConfiguration<PropertyTypeGroupDto>
{
    public void Configure(EntityTypeBuilder<PropertyTypeGroupDto> builder)
    {
        builder.ToTable(PropertyTypeGroupDto.TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName(PropertyTypeGroupDto.PrimaryKeyColumnName)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UniqueId)
            .HasColumnName(PropertyTypeGroupDto.UniqueIdColumnName);

        builder.Property(x => x.ContentTypeNodeId)
            .HasColumnName(PropertyTypeGroupDto.ContentTypeNodeIdColumnName);

        builder.Property(x => x.Type)
            .HasColumnName(PropertyTypeGroupDto.TypeColumnName)
            .HasDefaultValue((short)0);

        builder.Property(x => x.Text)
            .HasColumnName(PropertyTypeGroupDto.TextColumnName);

        builder.Property(x => x.Alias)
            .HasColumnName(PropertyTypeGroupDto.AliasColumnName);

        builder.Property(x => x.SortOrder)
            .HasColumnName(PropertyTypeGroupDto.SortOrderColumnName);

        // IX_cmsPropertyTypeGroupUniqueID (unique on uniqueID)
        builder.HasIndex(x => x.UniqueId)
            .IsUnique()
            .HasDatabaseName("IX_cmsPropertyTypeGroupUniqueID");

        // No EF Core navigation is declared for the content-type FK: it references the alternate key
        // (nodeId) rather than its primary key.
        // TODO (EF Core): the FK to cmsContentType.nodeId is currently created by NPoco's schema; revisit this comment once NPoco is removed.
    }
}
