using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class TagRelationshipDtoConfiguration : IEntityTypeConfiguration<TagRelationshipDto>
{
    public void Configure(EntityTypeBuilder<TagRelationshipDto> builder)
    {
        builder.ToTable(TagRelationshipDto.TableName);

        builder.HasKey(x => new { x.NodeId, x.PropertyTypeId, x.TagId })
            .HasName("PK_cmsTagRelationship");

        builder.Property(x => x.NodeId)
            .HasColumnName(TagRelationshipDto.NodeIdColumnName);

        builder.Property(x => x.TagId)
            .HasColumnName(TagRelationshipDto.TagIdColumnName);

        builder.Property(x => x.PropertyTypeId)
            .HasColumnName(TagRelationshipDto.PropertyTypeIdColumnName);

        // IX_cmsTagRelationship_tagId_nodeId
        // Note: SQL Server included column (propertyTypeId) is added by SqlServerTagRelationshipDtoModelCustomizer.
        builder.HasIndex(x => new { x.TagId, x.NodeId })
            .HasDatabaseName($"IX_{TagRelationshipDto.TableName}_tagId_nodeId");

        // FK: NodeId -> umbracoContent.nodeId
        builder.HasOne<ContentDto>()
            .WithMany()
            .HasForeignKey(x => x.NodeId)
            .HasConstraintName($"FK_{TagRelationshipDto.TableName}_cmsContent")
            .OnDelete(DeleteBehavior.NoAction);

        // FK: TagId -> cmsTags.id
        builder.HasOne<TagDto>()
            .WithMany()
            .HasForeignKey(x => x.TagId)
            .HasConstraintName($"FK_{TagRelationshipDto.TableName}_{TagDto.TableName}_id")
            .OnDelete(DeleteBehavior.NoAction);

        // FK: PropertyTypeId -> cmsPropertyType.id
        builder.HasOne<PropertyTypeDto>()
            .WithMany()
            .HasForeignKey(x => x.PropertyTypeId)
            .HasConstraintName($"FK_{TagRelationshipDto.TableName}_cmsPropertyType")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
