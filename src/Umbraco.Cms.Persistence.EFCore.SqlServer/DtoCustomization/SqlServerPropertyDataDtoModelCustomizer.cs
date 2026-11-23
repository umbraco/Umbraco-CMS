using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;

namespace Umbraco.Cms.Persistence.EFCore.SqlServer.DtoCustomization;

/// <summary>
/// Removes the filter the SQL Server provider adds by convention to the unique <see cref="PropertyDataDto"/> index
/// over nullable columns, so the model matches the unfiltered index the database has.
/// </summary>
public class SqlServerPropertyDataDtoModelCustomizer : IEFCoreModelCustomizer<PropertyDataDto>
{
    public string? ProviderName => Constants.ProviderNames.EFCore.SQLServer;

    public void Customize(EntityTypeBuilder<PropertyDataDto> builder) =>
        builder.HasIndex(x => new { x.VersionId, x.PropertyTypeId, x.LanguageId, x.Segment })
            .IsUnique()
            .HasDatabaseName($"IX_{PropertyDataDto.TableName}_VersionId")
            .HasFilter(null);
}
