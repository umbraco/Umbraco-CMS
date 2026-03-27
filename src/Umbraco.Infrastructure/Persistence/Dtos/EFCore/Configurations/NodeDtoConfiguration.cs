using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class NodeDtoConfiguration : IEntityTypeConfiguration<NodeDto>
{
    public void Configure(EntityTypeBuilder<NodeDto> builder)
    {
        builder.ToTable(NodeDto.TableName);

        builder.HasKey(x => x.NodeId);

        builder.Property(x => x.NodeId)
            .HasColumnName(NodeDto.IdColumnName)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UniqueId)
            .HasColumnName(NodeDto.KeyColumnName);

        builder.Property(x => x.ParentId)
            .HasColumnName(NodeDto.ParentIdColumnName);

        builder.Property(x => x.Level)
            .HasColumnName(NodeDto.LevelColumnName);

        builder.Property(x => x.Path)
            .HasColumnName(NodeDto.PathColumnName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .HasColumnName(NodeDto.SortOrderColumnName);

        builder.Property(x => x.Trashed)
            .HasColumnName(NodeDto.TrashedColumnName)
            .HasDefaultValue(false);

        builder.Property(x => x.UserId)
            .HasColumnName(NodeDto.UserIdColumnName);

        builder.Property(x => x.Text)
            .HasColumnName(NodeDto.TextColumnName);

        builder.Property(x => x.NodeObjectType)
            .HasColumnName(NodeDto.NodeObjectTypeColumnName);

        builder.Property(x => x.CreateDate)
            .HasColumnName(NodeDto.CreateDateColumnName);

        // Self-referencing FK: ParentId -> NodeId
        builder.HasOne<NodeDto>()
            .WithMany()
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.NoAction);

        // IX_umbracoNode_UniqueId (unique)
        // Note: SQL Server included columns are added by SqlServerNodeDtoModelCustomizer.
        builder.HasIndex(x => x.UniqueId)
            .IsUnique()
            .HasDatabaseName($"IX_{NodeDto.TableName}_UniqueId");

        // IX_umbracoNode_parentId_nodeObjectType
        // Note: SQL Server included columns are added by SqlServerNodeDtoModelCustomizer.
        builder.HasIndex(x => new { x.ParentId, x.NodeObjectType })
            .HasDatabaseName($"IX_{NodeDto.TableName}_parentId_nodeObjectType");

        // IX_umbracoNode_Level
        // Note: SQL Server included columns are added by SqlServerNodeDtoModelCustomizer.
        builder.HasIndex(x => new { x.Level, x.ParentId, x.SortOrder, x.NodeObjectType, x.Trashed })
            .HasDatabaseName($"IX_{NodeDto.TableName}_Level");

        // IX_umbracoNode_Path
        builder.HasIndex(x => x.Path)
            .HasDatabaseName($"IX_{NodeDto.TableName}_Path");

        // IX_umbracoNode_ObjectType_trashed_sorted
        // Note: SQL Server included columns are added by SqlServerNodeDtoModelCustomizer.
        builder.HasIndex(x => new { x.NodeObjectType, x.Trashed, x.SortOrder, x.NodeId })
            .HasDatabaseName($"IX_{NodeDto.TableName}_ObjectType_trashed_sorted");

        // IX_umbracoNode_Trashed
        builder.HasIndex(x => x.Trashed)
            .HasDatabaseName($"IX_{NodeDto.TableName}_Trashed");

        // IX_umbracoNode_ObjectType
        // Note: SQL Server included columns are added by SqlServerNodeDtoModelCustomizer.
        builder.HasIndex(x => new { x.NodeObjectType, x.Trashed })
            .HasDatabaseName($"IX_{NodeDto.TableName}_ObjectType");
    }
}
