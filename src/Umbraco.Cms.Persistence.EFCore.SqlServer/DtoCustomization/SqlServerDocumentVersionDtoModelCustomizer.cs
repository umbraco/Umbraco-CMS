using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;

namespace Umbraco.Cms.Persistence.EFCore.SqlServer.DtoCustomization;

/// <summary>
///     Applies SQL Server-specific model configuration for <see cref="DocumentVersionDto" />.
/// </summary>
public class SqlServerDocumentVersionDtoModelCustomizer : IEFCoreModelCustomizer<DocumentVersionDto>
{
    /// <inheritdoc />
    public void Customize(EntityTypeBuilder<DocumentVersionDto> builder)
    {
        builder
            .HasIndex(x => new { x.Id, x.Published })
            .HasDatabaseName($"IX_{DocumentVersionDto.TableName}_id_published")
            .IncludeProperties(x => new { x.TemplateId });

        builder
            .HasIndex(x => x.Published)
            .HasDatabaseName($"IX_{DocumentVersionDto.TableName}_published")
            .IncludeProperties(x => new { x.Id, x.TemplateId });
    }
}
