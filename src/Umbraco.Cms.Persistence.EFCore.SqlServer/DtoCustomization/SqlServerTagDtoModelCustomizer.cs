using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;

namespace Umbraco.Cms.Persistence.EFCore.SqlServer.DtoCustomization;

/// <summary>
/// Adds SQL Server-specific included columns to <see cref="TagDto"/> indexes.
/// </summary>
public class SqlServerTagDtoModelCustomizer : IEFCoreModelCustomizer<TagDto>
{
    public string? ProviderName => Constants.ProviderNames.SQLServer;

    public void Customize(EntityTypeBuilder<TagDto> builder)
    {
        // IX_cmsTags_languageId_group (includes id, tag)
        builder.HasIndex(x => new { x.LanguageId, x.Group })
            .HasDatabaseName($"IX_{TagDto.TableName}_languageId_group")
            .IncludeProperties(x => new { x.Id, x.Text });
    }
}
