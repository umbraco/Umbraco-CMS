// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_4_0;

/// <summary>
/// Adds an index on the versionDate column of the umbracoContentVersion table
/// to support efficient date-based filtering and ordering in the content version cleanup job.
/// </summary>
public class AddContentVersionDateIndex : AsyncMigrationBase
{
    private const string IndexName = "IX_" + ContentVersionDto.TableName + "_VersionDate";

    /// <summary>
    /// Initializes a new instance of the <see cref="AddContentVersionDateIndex"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public AddContentVersionDateIndex(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc/>
    protected override async Task MigrateAsync()
    {
        if (IndexExists(IndexName))
        {
            return;
        }

        // Give scope for the migration to complete within the command timeout, which may be necessary on large datasets.
        EnsureLongCommandTimeout(Database);

        // CREATE INDEX (without NONCLUSTERED) is portable across SQL Server and SQLite.
        // SQL Server defaults to nonclustered; SQLite does not support the NONCLUSTERED keyword.
        Execute.Sql($@"
            CREATE INDEX [{IndexName}]
            ON [{ContentVersionDto.TableName}] ([versionDate])
        ").Do();
    }
}
