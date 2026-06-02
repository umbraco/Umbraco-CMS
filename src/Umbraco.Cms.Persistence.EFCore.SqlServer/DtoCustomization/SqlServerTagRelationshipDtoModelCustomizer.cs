using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;

namespace Umbraco.Cms.Persistence.EFCore.SqlServer.DtoCustomization;

/// <summary>
/// Adds SQL Server-specific included columns to <see cref="TagRelationshipDto"/> indexes.
/// </summary>
public class SqlServerTagRelationshipDtoModelCustomizer : IEFCoreModelCustomizer<TagRelationshipDto>
{
    public string? ProviderName => Constants.ProviderNames.SQLServer;

    public void Customize(EntityTypeBuilder<TagRelationshipDto> builder)
    {
        // IX_cmsTagRelationship_tagId_nodeId (includes propertyTypeId)
        builder.HasIndex(x => new { x.TagId, x.NodeId })
            .HasDatabaseName($"IX_{TagRelationshipDto.TableName}_tagId_nodeId")
            .IncludeProperties(x => x.PropertyTypeId);
    }
}
