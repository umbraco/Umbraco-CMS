using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

internal class NodeDtoConfiguration : IEntityTypeConfiguration<NodeDto>
{
    public void Configure(EntityTypeBuilder<NodeDto> builder)
    {
        builder.ToTable(NodeDto.TableName);

        builder.HasKey(x => x.NodeId);

        builder.Property(x => x.NodeId)
            .HasColumnName(NodeDto.PrimaryKeyColumnName)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UniqueId)
            .HasColumnName(NodeDto.UniqueIdColumnName)
            .IsRequired();

        builder.Property(x => x.ParentId)
            .HasColumnName(NodeDto.ParentIdColumnName)
            .IsRequired();

        builder.Property(x => x.Level)
            .HasColumnName(NodeDto.LevelColumnName)
            .IsRequired();

        builder.Property(x => x.Path)
            .HasColumnName(NodeDto.PathColumnName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .HasColumnName(NodeDto.SortOrderColumnName)
            .IsRequired();

        builder.Property(x => x.Trashed)
            .HasColumnName(NodeDto.TrashedColumnName)
            .IsRequired();

        builder.Property(x => x.UserId)
            .HasColumnName(NodeDto.UserIdColumnName);

        builder.Property(x => x.Text)
            .HasColumnName(NodeDto.TextColumnName);

        builder.Property(x => x.NodeObjectType)
            .HasColumnName(NodeDto.NodeObjectTypeColumnName);

        builder.Property(x => x.CreateDate)
            .HasColumnName(NodeDto.CreateDateColumnName)
            .IsRequired();

        // TODO: Figure out how to add included columns
        // https://learn.microsoft.com/en-us/ef/core/modeling/indexes?tabs=fluent-api#included-columns
        // IX_umbracoNode_UniqueId
        builder.HasIndex(x => x.UniqueId)
            .IsUnique()
            .HasDatabaseName($"IX_{NodeDto.TableName}_UniqueId");

        // IX_umbracoNode_parentId_nodeObjectType
        builder.HasIndex(x => new { x.ParentId, x.NodeObjectType })
            .HasDatabaseName($"IX_{NodeDto.TableName}_parentId_nodeObjectType");

        // IX_umbracoNode_Level
        builder.HasIndex(x => new { x.Level, x.ParentId, x.SortOrder, x.NodeObjectType, x.Trashed })
            .HasDatabaseName($"IX_{NodeDto.TableName}_Level");

        // IX_umbracoNode_Path
        builder.HasIndex(x => x.Path)
            .HasDatabaseName($"IX_{NodeDto.TableName}_Path");

        // IX_umbracoNode_ObjectType_trashed_sorted
        builder.HasIndex(x => new { x.NodeObjectType, x.Trashed, x.SortOrder, x.NodeId })
            .HasDatabaseName($"IX_{NodeDto.TableName}_ObjectType_trashed_sorted");

        // IX_umbracoNode_Trashed
        builder.HasIndex(x => x.Trashed)
            .HasDatabaseName($"IX_{NodeDto.TableName}_Trashed");

        // IX_umbracoNode_ObjectType
        builder.HasIndex(x => new { x.NodeObjectType, x.Trashed })
            .HasDatabaseName($"IX_{NodeDto.TableName}_ObjectType");
    }
}
