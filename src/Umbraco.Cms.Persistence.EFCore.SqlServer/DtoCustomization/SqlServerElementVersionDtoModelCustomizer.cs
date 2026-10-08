using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;

namespace Umbraco.Cms.Persistence.EFCore.SqlServer.DtoCustomization;

/// <summary>
/// Adds SQL Server-specific included columns to <see cref="ElementVersionDto"/> indexes.
/// </summary>
public class SqlServerElementVersionDtoModelCustomizer : IEFCoreModelCustomizer<ElementVersionDto>
{
    public string? ProviderName => Constants.ProviderNames.EFCore.SQLServer;

    public void Customize(EntityTypeBuilder<ElementVersionDto> builder)
    {
        // IX_umbracoElementVersion_published (on Published)
        builder.HasIndex(x => x.Published)
            .HasDatabaseName($"IX_{ElementVersionDto.TableName}_published")
            .IncludeProperties(x => new { x.Id });
    }
}
