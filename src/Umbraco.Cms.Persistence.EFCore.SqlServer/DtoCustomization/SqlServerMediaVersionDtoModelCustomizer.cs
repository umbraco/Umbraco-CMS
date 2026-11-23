using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;

namespace Umbraco.Cms.Persistence.EFCore.SqlServer.DtoCustomization;

/// <summary>
/// Removes the filter the SQL Server provider adds by convention to the unique <see cref="MediaVersionDto"/> index
/// over nullable columns, so the model matches the unfiltered index the database has.
/// </summary>
public class SqlServerMediaVersionDtoModelCustomizer : IEFCoreModelCustomizer<MediaVersionDto>
{
    public string? ProviderName => Constants.ProviderNames.EFCore.SQLServer;

    public void Customize(EntityTypeBuilder<MediaVersionDto> builder) =>
        builder.HasIndex(x => new { x.Id, x.Path })
            .IsUnique()
            .HasDatabaseName($"IX_{MediaVersionDto.TableName}")
            .HasFilter(null);
}
