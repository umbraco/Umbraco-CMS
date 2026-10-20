// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_6_0;

/// <summary>
/// Adds an index on the contentTypeId column of the umbracoContent table to support efficient
/// content-type-scoped queries (e.g. the content cache rebuild triggered by content type changes).
/// </summary>
public class AddContentTypeIdIndexForContent : AsyncMigrationBase
{
    private const string IndexName = "IX_" + ContentDto.TableName + "_" + ContentDto.ContentTypeIdColumnName;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddContentTypeIdIndexForContent"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public AddContentTypeIdIndexForContent(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc/>
    protected override Task MigrateAsync()
    {
        if (IndexExists(IndexName))
        {
            return Task.CompletedTask;
        }

        // Give scope for the migration to complete within the command timeout, which may be necessary on large datasets.
        EnsureLongCommandTimeout(Database);

        // Create the index from the definition on ContentDto, so it matches a fresh install exactly.
        CreateIndex<ContentDto>(IndexName);

        return Task.CompletedTask;
    }
}
