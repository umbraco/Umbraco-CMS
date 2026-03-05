using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_3_0;

/// <summary>
/// Optimizes URL and alias storage for invariant documents by making languageId nullable.
/// Invariant content will use NULL languageId instead of duplicating records for each language.
/// </summary>
public class OptimizeInvariantUrlRecords : AsyncMigrationBase
{
    private readonly IKeyValueService _keyValueService;

    /// <summary>
    /// Initializes a new instance of the <see cref="OptimizeInvariantUrlRecords"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    /// <param name="keyValueService">The key value service.</param>
    public OptimizeInvariantUrlRecords(IMigrationContext context, IKeyValueService keyValueService)
        : base(context)
    {
        _keyValueService = keyValueService;
    }

    /// <inheritdoc/>
    protected override async Task MigrateAsync()
    {
        if (DatabaseType == DatabaseType.SQLite)
        {
            MigrateSqlite();
        }
        else
        {
            MigrateSqlServer();
        }
    }

    private void MigrateSqlServer()
    {
        // Make languageId nullable in umbracoDocumentUrl.
        AlterDocumentUrlLanguageIdToNullable();

        // Make languageId nullable in umbracoDocumentUrlAlias.
        AlterDocumentUrlAliasLanguageIdToNullable();

        // Convert existing invariant records to use NULL languageId and remove duplicates.
        ConvertInvariantDocumentUrlRecords();
        ConvertInvariantDocumentUrlAliasRecords();

        // Trigger rebuild to update the in-memory cache with new structure.
        TriggerRebuild();
    }

    private void MigrateSqlite()
    {
        // SQLite doesn't support ALTER TABLE to modify columns well.
        // Instead, we drop and recreate the tables, then trigger a full rebuild on startup.

        // Drop existing tables.
        if (TableExists(Constants.DatabaseSchema.Tables.DocumentUrl))
        {
            Delete.Table(Constants.DatabaseSchema.Tables.DocumentUrl).Do();
        }

        if (TableExists(Constants.DatabaseSchema.Tables.DocumentUrlAlias))
        {
            Delete.Table(Constants.DatabaseSchema.Tables.DocumentUrlAlias).Do();
        }

        // Recreate tables with nullable languageId (using updated DTOs).
        Create.Table<DocumentUrlDto>().Do();
        Create.Table<DocumentUrlAliasDto>().Do();

        // Trigger rebuild on startup to repopulate the tables
        TriggerRebuild();
    }

    private void AlterDocumentUrlLanguageIdToNullable()
    {
        var tableName = Constants.DatabaseSchema.Tables.DocumentUrl;
        var columnName = DocumentUrlDto.LanguageIdColumnName;

        // Drop the existing unique clustered index that includes languageId.
        var existingIndexName = $"IX_{tableName}";
        if (IndexExists(existingIndexName))
        {
            Delete.Index(existingIndexName).OnTable(tableName).Do();
        }

        // Alter the column to be nullable
        Alter.Table(tableName)
            .AlterColumn(columnName)
            .AsInt32()
            .Nullable()
            .Do();

        // Recreate the unique clustered index (now works with NULL values for invariant content).
        Execute.Sql($@"
            CREATE UNIQUE CLUSTERED INDEX [IX_{tableName}]
            ON [{tableName}] ([{DocumentUrlDto.UniqueIdColumnName}], [{columnName}], [{DocumentUrlDto.IsDraftColumnName}], [{DocumentUrlDto.UrlSegmentColumnName}])
        ").Do();
    }

    private void AlterDocumentUrlAliasLanguageIdToNullable()
    {
        var tableName = Constants.DatabaseSchema.Tables.DocumentUrlAlias;
        var columnName = "languageId";

        // Drop the existing unique index.
        var existingUniqueIndexName = $"IX_{tableName}_Unique";
        if (IndexExists(existingUniqueIndexName))
        {
            Delete.Index(existingUniqueIndexName).OnTable(tableName).Do();
        }

        // Drop the lookup index.
        var existingLookupIndexName = $"IX_{tableName}_Lookup";
        if (IndexExists(existingLookupIndexName))
        {
            Delete.Index(existingLookupIndexName).OnTable(tableName).Do();
        }

        // Alter the column to be nullable.
        Alter.Table(tableName)
            .AlterColumn(columnName)
            .AsInt32()
            .Nullable()
            .Do();

        // Recreate the unique index.
        Execute.Sql($@"
            CREATE UNIQUE NONCLUSTERED INDEX [IX_{tableName}_Unique]
            ON [{tableName}] ([uniqueId], [{columnName}], [alias])
        ").Do();

        // Recreate the lookup index.
        Execute.Sql($@"
            CREATE NONCLUSTERED INDEX [IX_{tableName}_Lookup]
            ON [{tableName}] ([alias], [{columnName}])
        ").Do();
    }

    private void ConvertInvariantDocumentUrlRecords()
    {
        // For SQL Server: Convert existing invariant records to use NULL languageId and remove duplicates.
        // Invariant documents are those with ContentVariation.Nothing (variations = 0) in cmsContentType.
        // Note: ContentVariation.Culture = 1, ContentVariation.Segment = 2, so 0 means no variation (invariant).
        Execute.Sql($@"
            -- Identify invariant documents
            ;WITH InvariantDocs AS (
                SELECT DISTINCT du.uniqueId
                FROM [{Constants.DatabaseSchema.Tables.DocumentUrl}] du
                INNER JOIN [{Constants.DatabaseSchema.Tables.Node}] n ON du.uniqueId = n.uniqueId
                INNER JOIN [{Constants.DatabaseSchema.Tables.Content}] c ON n.id = c.nodeId
                INNER JOIN [{Constants.DatabaseSchema.Tables.ContentType}] ct ON c.contentTypeId = ct.nodeId
                WHERE ct.variations = 0  -- ContentVariation.Nothing (invariant)
            ),
            -- Select one record per unique combination to keep
            ToKeep AS (
                SELECT MIN(du.id) AS id, du.uniqueId, du.isDraft, du.urlSegment, du.isPrimary
                FROM [{Constants.DatabaseSchema.Tables.DocumentUrl}] du
                INNER JOIN InvariantDocs i ON du.uniqueId = i.uniqueId
                GROUP BY du.uniqueId, du.isDraft, du.urlSegment, du.isPrimary
            )
            -- Delete duplicates (keep one per unique combination)
            DELETE du FROM [{Constants.DatabaseSchema.Tables.DocumentUrl}] du
            INNER JOIN InvariantDocs i ON du.uniqueId = i.uniqueId
            WHERE du.id NOT IN (SELECT id FROM ToKeep);
        ").Do();

        Execute.Sql($@"
            -- Set languageId = NULL for remaining invariant records
            ;WITH InvariantDocs AS (
                SELECT DISTINCT du.uniqueId
                FROM [{Constants.DatabaseSchema.Tables.DocumentUrl}] du
                INNER JOIN [{Constants.DatabaseSchema.Tables.Node}] n ON du.uniqueId = n.uniqueId
                INNER JOIN [{Constants.DatabaseSchema.Tables.Content}] c ON n.id = c.nodeId
                INNER JOIN [{Constants.DatabaseSchema.Tables.ContentType}] ct ON c.contentTypeId = ct.nodeId
                WHERE ct.variations = 0  -- ContentVariation.Nothing (invariant)
            )
            UPDATE du SET du.languageId = NULL
            FROM [{Constants.DatabaseSchema.Tables.DocumentUrl}] du
            INNER JOIN InvariantDocs i ON du.uniqueId = i.uniqueId;
        ").Do();
    }

    private void ConvertInvariantDocumentUrlAliasRecords()
    {
        // For SQL Server: Convert existing invariant alias records to use NULL languageId and remove duplicates.
        // Note: ContentVariation.Nothing = 0 (invariant), ContentVariation.Culture = 1 (variant).
        Execute.Sql($@"
            -- Identify invariant documents with aliases
            ;WITH InvariantDocs AS (
                SELECT DISTINCT da.uniqueId
                FROM [{Constants.DatabaseSchema.Tables.DocumentUrlAlias}] da
                INNER JOIN [{Constants.DatabaseSchema.Tables.Node}] n ON da.uniqueId = n.uniqueId
                INNER JOIN [{Constants.DatabaseSchema.Tables.Content}] c ON n.id = c.nodeId
                INNER JOIN [{Constants.DatabaseSchema.Tables.ContentType}] ct ON c.contentTypeId = ct.nodeId
                WHERE ct.variations = 0  -- ContentVariation.Nothing (invariant)
            ),
            -- Select one record per unique combination to keep
            ToKeep AS (
                SELECT MIN(da.id) AS id, da.uniqueId, da.alias
                FROM [{Constants.DatabaseSchema.Tables.DocumentUrlAlias}] da
                INNER JOIN InvariantDocs i ON da.uniqueId = i.uniqueId
                GROUP BY da.uniqueId, da.alias
            )
            -- Delete duplicates (keep one per unique combination)
            DELETE da FROM [{Constants.DatabaseSchema.Tables.DocumentUrlAlias}] da
            INNER JOIN InvariantDocs i ON da.uniqueId = i.uniqueId
            WHERE da.id NOT IN (SELECT id FROM ToKeep);
        ").Do();

        Execute.Sql($@"
            -- Set languageId = NULL for remaining invariant alias records
            ;WITH InvariantDocs AS (
                SELECT DISTINCT da.uniqueId
                FROM [{Constants.DatabaseSchema.Tables.DocumentUrlAlias}] da
                INNER JOIN [{Constants.DatabaseSchema.Tables.Node}] n ON da.uniqueId = n.uniqueId
                INNER JOIN [{Constants.DatabaseSchema.Tables.Content}] c ON n.id = c.nodeId
                INNER JOIN [{Constants.DatabaseSchema.Tables.ContentType}] ct ON c.contentTypeId = ct.nodeId
                WHERE ct.variations = 0  -- ContentVariation.Nothing (invariant)
            )
            UPDATE da SET da.languageId = NULL
            FROM [{Constants.DatabaseSchema.Tables.DocumentUrlAlias}] da
            INNER JOIN InvariantDocs i ON da.uniqueId = i.uniqueId;
        ").Do();
    }

    private void TriggerRebuild()
    {
        // Clear the rebuild keys to trigger a full rebuild on next startup.
        _keyValueService.SetValue(DocumentUrlService.RebuildKey, string.Empty);
        _keyValueService.SetValue(DocumentUrlAliasService.RebuildKey, string.Empty);
    }
}
