using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;

namespace Umbraco.Cms.Persistence.EFCore.SqlServer.DtoCustomization;

/// <summary>
/// Adds SQL Server-specific included columns to <see cref="ContentVersionDto"/> indexes.
/// </summary>
public class SqlServerContentVersionDtoModelCustomizer : IEFCoreModelCustomizer<ContentVersionDto>
{
    public string? ProviderName => Constants.ProviderNames.SQLServer;

    public void Customize(EntityTypeBuilder<ContentVersionDto> builder)
    {
        // IX_umbracoContentVersion_NodeId (composite on NodeId+Current)
        builder.HasIndex(x => new { x.NodeId, x.Current })
            .HasDatabaseName($"IX_{ContentVersionDto.TableName}_NodeId")
            .IncludeProperties(x => new { x.Id, x.VersionDate, x.Text, x.UserId, x.PreventCleanup });

        // IX_umbracoContentVersion_Current
        builder.HasIndex(x => x.Current)
            .HasDatabaseName($"IX_{ContentVersionDto.TableName}_Current")
            .IncludeProperties(x => new { x.NodeId });
    }
}
