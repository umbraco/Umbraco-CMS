using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;

namespace Umbraco.Cms.Persistence.EFCore.SqlServer.DtoCustomization;

/// <summary>
/// Adds SQL Server-specific included columns to <see cref="DocumentVersionDto"/> indexes.
/// </summary>
public class SqlServerDocumentVersionDtoModelCustomizer : IEFCoreModelCustomizer<DocumentVersionDto>
{
    public string? ProviderName => Constants.ProviderNames.SQLServer;

    public void Customize(EntityTypeBuilder<DocumentVersionDto> builder)
    {
        // IX_umbracoDocumentVersion_id_published (composite on Id+Published)
        builder.HasIndex(x => new { x.Id, x.Published })
            .HasDatabaseName($"IX_{DocumentVersionDto.TableName}_id_published")
            .IncludeProperties(x => new { x.TemplateId });

        // IX_umbracoDocumentVersion_published (on Published)
        builder.HasIndex(x => x.Published)
            .HasDatabaseName($"IX_{DocumentVersionDto.TableName}_published")
            .IncludeProperties(x => new { x.Id, x.TemplateId });
    }
}
