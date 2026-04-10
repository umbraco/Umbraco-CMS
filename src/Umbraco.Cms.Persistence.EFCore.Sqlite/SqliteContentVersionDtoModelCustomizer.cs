using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;

namespace Umbraco.Cms.Persistence.EFCore.Sqlite;

/// <summary>
/// Overrides the <c>versionDate</c> column default for SQLite, replacing the SQL Server-specific
/// <c>GETUTCDATE()</c> expression with the SQLite equivalent <c>datetime('now')</c>.
/// </summary>
public class SqliteContentVersionDtoModelCustomizer : IEFCoreModelCustomizer<ContentVersionDto>
{
    /// <inheritdoc />
    public string? ProviderName => Constants.ProviderNames.EFCore.SQLite;

    /// <inheritdoc />
    public void Customize(EntityTypeBuilder<ContentVersionDto> builder) =>
        builder.Property(x => x.VersionDate)
            .HasDefaultValueSql("datetime('now')");
}
