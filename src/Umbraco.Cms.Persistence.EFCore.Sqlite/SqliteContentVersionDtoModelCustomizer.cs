using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;

namespace Umbraco.Cms.Persistence.EFCore.Sqlite;

/// <summary>
/// Overrides the <c>versionDate</c> column default for SQLite, matching the <c>CURRENT_TIMESTAMP</c>
/// the SQLite syntax provider writes for a current-UTC default.
/// </summary>
public class SqliteContentVersionDtoModelCustomizer : IEFCoreModelCustomizer<ContentVersionDto>
{
    /// <inheritdoc />
    public string? ProviderName => Constants.ProviderNames.EFCore.SQLite;

    /// <inheritdoc />
    public void Customize(EntityTypeBuilder<ContentVersionDto> builder) =>
        builder.Property(x => x.VersionDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
}
