// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
/// Migration to add the search index document table used by Umbraco Search.
/// </summary>
public class AddIndexDocumentTable : AsyncMigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddIndexDocumentTable"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public AddIndexDocumentTable(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc/>
    protected override Task MigrateAsync()
    {
        // The table may already exist when upgrading from a site that used the standalone
        // Umbraco.Cms.Search package (it was created by a package migration there).
        if (TableExists(Constants.DatabaseSchema.Tables.IndexDocument) is false)
        {
            Create.Table<IndexDocumentDto>().Do();
        }

        return Task.CompletedTask;
    }
}
