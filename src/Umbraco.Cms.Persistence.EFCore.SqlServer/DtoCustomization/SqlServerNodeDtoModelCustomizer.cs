using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;

namespace Umbraco.Cms.Persistence.EFCore.SqlServer.DtoCustomization;

/// <summary>
/// Adds SQL Server-specific included columns to <see cref="NodeDto"/> indexes.
/// </summary>
public class SqlServerNodeDtoModelCustomizer : IEFCoreModelCustomizer<NodeDto>
{
    public string? ProviderName => "Microsoft.EntityFrameworkCore.SqlServer";

    public void Customize(EntityTypeBuilder<NodeDto> builder)
    {
        // IX_umbracoNode_UniqueId
        builder.HasIndex(x => x.UniqueId)
            .IsUnique()
            .HasDatabaseName($"IX_{NodeDto.TableName}_UniqueId")
            .IncludeProperties(x => new { x.ParentId, x.Level, x.Path, x.SortOrder, x.Trashed, x.UserId, x.Text, x.CreateDate });

        // IX_umbracoNode_parentId_nodeObjectType
        builder.HasIndex(x => new { x.ParentId, x.NodeObjectType })
            .HasDatabaseName($"IX_{NodeDto.TableName}_parentId_nodeObjectType")
            .IncludeProperties(x => new { x.Trashed, x.UserId, x.Level, x.Path, x.SortOrder, x.UniqueId, x.Text, x.CreateDate });

        // IX_umbracoNode_Level
        builder.HasIndex(x => new { x.Level, x.ParentId, x.SortOrder, x.NodeObjectType, x.Trashed })
            .HasDatabaseName($"IX_{NodeDto.TableName}_Level")
            .IncludeProperties(x => new { x.UserId, x.Path, x.UniqueId, x.CreateDate });

        // IX_umbracoNode_ObjectType_trashed_sorted
        builder.HasIndex(x => new { x.NodeObjectType, x.Trashed, x.SortOrder, x.NodeId })
            .HasDatabaseName($"IX_{NodeDto.TableName}_ObjectType_trashed_sorted")
            .IncludeProperties(x => new { x.UniqueId, x.ParentId, x.Level, x.Path, x.UserId, x.Text, x.CreateDate });

        // IX_umbracoNode_ObjectType
        builder.HasIndex(x => new { x.NodeObjectType, x.Trashed })
            .HasDatabaseName($"IX_{NodeDto.TableName}_ObjectType")
            .IncludeProperties(x => new { x.UniqueId, x.ParentId, x.Level, x.Path, x.SortOrder, x.UserId, x.Text, x.CreateDate });
    }
}
