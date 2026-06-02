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
            .HasColumnName("type")
            .HasDefaultValue((short)0);

        builder.Property(x => x.Text)
            .HasColumnName("text");

        builder.Property(x => x.Alias)
            .HasColumnName("alias")
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .HasColumnName("sortorder");

        // IX_cmsPropertyTypeGroupUniqueID
        builder.HasIndex(x => x.UniqueId)
            .IsUnique()
            .HasDatabaseName($"IX_{PropertyTypeGroupDto.TableName}UniqueID");

        // FK: ContentTypeNodeId -> cmsContentType.nodeId (non-PK, unique column on ContentTypeDto)
        builder.HasOne<ContentTypeDto>()
            .WithMany()
            .HasForeignKey(x => x.ContentTypeNodeId)
            .HasPrincipalKey(nameof(ContentTypeDto.NodeId))
            .OnDelete(DeleteBehavior.Cascade);
    }
}
