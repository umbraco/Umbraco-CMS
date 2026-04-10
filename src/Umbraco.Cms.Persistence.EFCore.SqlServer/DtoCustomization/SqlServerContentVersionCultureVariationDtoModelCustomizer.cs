using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;

namespace Umbraco.Cms.Persistence.EFCore.SqlServer.DtoCustomization;

/// <summary>
/// Adds SQL Server-specific included columns to <see cref="ContentVersionCultureVariationDto"/> indexes.
/// </summary>
public class SqlServerContentVersionCultureVariationDtoModelCustomizer : IEFCoreModelCustomizer<ContentVersionCultureVariationDto>
{
    public string? ProviderName => Constants.ProviderNames.SQLServer;

    public void Customize(EntityTypeBuilder<ContentVersionCultureVariationDto> builder)
    {
        // IX_umbracoContentVersionCultureVariation_VersionId (unique, composite on VersionId+LanguageId)
        builder.HasIndex(x => new { x.VersionId, x.LanguageId })
            .IsUnique()
            .HasDatabaseName($"IX_{ContentVersionCultureVariationDto.TableName}_VersionId")
            .IncludeProperties(x => new { x.Id, x.Name, x.UpdateDate, x.UpdateUserId });
    }
}
