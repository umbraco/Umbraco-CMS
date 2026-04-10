using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;

namespace Umbraco.Cms.Persistence.EFCore.SqlServer.DtoCustomization;

/// <summary>
/// Configures SQL Server-specific clustered index behavior for <see cref="DocumentUrlDto"/>.
/// The primary key is non-clustered and the unique index on (UniqueId, LanguageId, IsDraft, UrlSegment) is clustered.
/// </summary>
public class SqlServerDocumentUrlDtoModelCustomizer : IEFCoreModelCustomizer<DocumentUrlDto>
{
    public string? ProviderName => Constants.ProviderNames.SQLServer;

    public void Customize(EntityTypeBuilder<DocumentUrlDto> builder)
    {
        // Make the primary key non-clustered
        builder.HasKey(x => x.NodeId)
            .IsClustered(false);

        // IX_umbracoDocumentUrl (unique clustered on UniqueId+LanguageId+IsDraft+UrlSegment)
        builder.HasIndex(x => new { x.UniqueId, x.LanguageId, x.IsDraft, x.UrlSegment })
            .IsUnique()
            .IsClustered()
            .HasDatabaseName($"IX_{DocumentUrlDto.TableName}");
    }
}
