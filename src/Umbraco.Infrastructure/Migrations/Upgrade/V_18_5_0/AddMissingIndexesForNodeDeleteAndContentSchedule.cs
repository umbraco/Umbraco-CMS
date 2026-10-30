// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_5_0;

/// <summary>
/// Adds indexes on columns that are filtered on when a node is deleted, and when content schedules are read or polled.
/// </summary>
/// <remarks>
/// Deleting a node removes rows from umbracoRelation (by childId), umbracoContentSchedule (by nodeId),
/// umbracoRedirectUrl (by contentKey), umbracoUser2NodeNotify (by nodeId) and umbracoUserStartNode (by startNode),
/// and the foreign key checks on the final delete from umbracoNode filter on the same columns. Without these indexes
/// each of those statements scans the table, once per deleted node. umbracoContentSchedule is also read by nodeId
/// when content is loaded, and polled by action and date by the scheduled publishing job.
/// </remarks>
public class AddMissingIndexesForNodeDeleteAndContentSchedule : AsyncMigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddMissingIndexesForNodeDeleteAndContentSchedule"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public AddMissingIndexesForNodeDeleteAndContentSchedule(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc/>
    protected override Task MigrateAsync()
    {
        // Give scope for the migration to complete within the command timeout, which may be necessary on large datasets.
        EnsureLongCommandTimeout(Database);

        // Create the indexes from the definitions on the DTOs, so they match a fresh install exactly.
        CreateIndexIfNotExists<RelationDto>(RelationDto.ChildIdIndexName);
        CreateIndexIfNotExists<ContentScheduleDto>(ContentScheduleDto.NodeIdIndexName);
        CreateIndexIfNotExists<ContentScheduleDto>(ContentScheduleDto.ActionDateIndexName);
        CreateIndexIfNotExists<RedirectUrlDto>(RedirectUrlDto.ContentKeyIndexName);
        CreateIndexIfNotExists<User2NodeNotifyDto>(User2NodeNotifyDto.NodeIdIndexName);
        CreateIndexIfNotExists<UserStartNodeDto>(UserStartNodeDto.StartNodeIndexName);

        return Task.CompletedTask;
    }

    private void CreateIndexIfNotExists<TDto>(string indexName)
    {
        if (IndexExists(indexName))
        {
            return;
        }

        CreateIndex<TDto>(indexName);
    }
}
