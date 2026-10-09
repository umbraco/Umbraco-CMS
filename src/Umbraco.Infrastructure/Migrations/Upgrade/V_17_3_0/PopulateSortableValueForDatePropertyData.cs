// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_3_0;

/// <summary>
/// Populates the sortableValue column for existing property data that uses date/time property editors
/// which store their values as JSON in the textValue column.
/// </summary>
public class PopulateSortableValueForDatePropertyData : AsyncMigrationBase
{
    // Updates are performed in batches to keep individual command durations bounded and to avoid
    // holding locks across the entire table for the full duration of the migration.
    private const int BatchSize = 10000;

    // Property editor aliases that store date/time as JSON and implement IDataValueSortable.
    private static readonly string[] _dateTimePropertyEditorAliases =
    [
        Constants.PropertyEditors.Aliases.DateTimeWithTimeZone,
        Constants.PropertyEditors.Aliases.DateTimeUnspecified,
        Constants.PropertyEditors.Aliases.DateOnly,
        Constants.PropertyEditors.Aliases.TimeOnly,
    ];

    private readonly ILogger<PopulateSortableValueForDatePropertyData> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PopulateSortableValueForDatePropertyData"/> class.
    /// </summary>
    public PopulateSortableValueForDatePropertyData(
        IMigrationContext context,
        ILogger<PopulateSortableValueForDatePropertyData> logger)
        : base(context)
        => _logger = logger;

    /// <inheritdoc/>
    protected override Task MigrateAsync()
    {
        EnsureLongCommandTimeout(Database);
        ExecuteMigration(Database, DatabaseType, _logger);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Executes the migration to populate sortable values for date property data.
    /// </summary>
    /// <param name="database">The database instance.</param>
    /// <param name="databaseType">The database type (SQLite or SQL Server).</param>
    /// <param name="logger">The logger instance.</param>
    /// <returns>The number of rows affected.</returns>
    public static int ExecuteMigration(IUmbracoDatabase database, DatabaseType databaseType, ILogger logger)
    {
        // Resolve the relevant property type ids up front so the UPDATE can filter with a static IN list.
        // A subquery here causes SQL Server to pick a scan-with-row-goal plan for the batched UPDATE,
        // evaluating TRY_CAST(JSON_VALUE(textValue, '$.date')) on every row of umbracoPropertyData before
        // the semi-join prunes anything — poor performance on large tables even when no rows ultimately match.
        // A static IN list lets the optimizer drive the query off the IX_umbracoPropertyData_PropertyTypeId
        // index, so TRY_CAST / datetime() only evaluate on rows already narrowed down by property type.
        List<int> propertyTypeIds = GetSortablePropertyTypeIds(database);
        if (propertyTypeIds.Count == 0)
        {
            logger.LogInformation(
                "Skipping sortableValue population; no property types using date/time editors were found.");
            return 0;
        }

        logger.LogInformation(
            "Populating sortableValue for property data across {PropertyTypeCount} date/time property type(s).",
            propertyTypeIds.Count);

        var idsInClause = string.Join(", ", propertyTypeIds);

        return databaseType == DatabaseType.SQLite
            ? MigrateSQLite(database, idsInClause, logger)
            : MigrateSqlServer(database, idsInClause, logger);
    }

    private static List<int> GetSortablePropertyTypeIds(IUmbracoDatabase database)
    {
        var aliasesInClause = string.Join(", ", _dateTimePropertyEditorAliases.Select(a => $"'{a}'"));
        var sql = $@"
SELECT id
FROM cmsPropertyType
WHERE dataTypeId IN (
    SELECT nodeId
    FROM umbracoDataType
    WHERE propertyEditorAlias IN ({aliasesInClause})
)";
        return database.Fetch<int>(sql);
    }

    /// <summary>
    /// SQL Server migration using JSON_VALUE and datetimeoffset for proper UTC conversion.
    /// </summary>
    /// <remarks>
    /// The JSON format is: {"date":"2025-11-05T15:31:00+00:00","timeZone":"UTC"}
    /// We extract the date, parse it as datetimeoffset, convert to UTC, and format as ISO 8601.
    /// The resulting format is: 2025-11-05T15:31:00.0000000+00:00.
    /// TRY_CAST is used so rows with unparseable dates are skipped rather than aborting the migration;
    /// the same guard in the WHERE clause ensures the batch loop always makes progress.
    /// The parsed datetimeoffset is computed once via CROSS APPLY and reused for both the filter
    /// and the assignment, avoiding a second JSON_VALUE + TRY_CAST per row.
    /// </remarks>
    private static int MigrateSqlServer(IUmbracoDatabase database, string propertyTypeIdsInClause, ILogger logger)
    {
        var sql = $@"
UPDATE TOP ({BatchSize}) pd
SET sortableValue = CONVERT(varchar(50), SWITCHOFFSET(parsed.parsedDate, '+00:00'), 127)
FROM umbracoPropertyData pd
CROSS APPLY (SELECT TRY_CAST(JSON_VALUE(pd.textValue, '$.date') AS datetimeoffset) AS parsedDate) parsed
WHERE pd.propertyTypeId IN ({propertyTypeIdsInClause})
AND pd.textValue IS NOT NULL
AND pd.sortableValue IS NULL
AND ISJSON(pd.textValue) = 1
AND parsed.parsedDate IS NOT NULL";

        return ExecuteInBatches(database, sql, logger, "SQL Server");
    }

    /// <summary>
    /// SQLite migration using json_extract for date extraction.
    /// </summary>
    /// <remarks>
    /// SQLite has limited datetime manipulation capabilities, so we extract the date string
    /// and use strftime to normalize it. For dates with timezone offsets, SQLite's datetime
    /// function can parse ISO 8601 format and converts to UTC.
    /// The resulting format is: yyyy-MM-ddTHH:mm:ssZ.
    /// The datetime() IS NOT NULL guard in the WHERE clause excludes unparseable dates so the
    /// batch loop always makes progress (writing NULL back would otherwise re-select the row).
    /// </remarks>
    private static int MigrateSQLite(IUmbracoDatabase database, string propertyTypeIdsInClause, ILogger logger)
    {
        // SQLite's default build does not support UPDATE ... LIMIT, so constrain the update via a
        // subquery that selects a batch of matching row ids.
        var sql = $@"
UPDATE umbracoPropertyData
SET sortableValue = strftime('%Y-%m-%dT%H:%M:%SZ', datetime(json_extract(textValue, '$.date')))
WHERE id IN (
    SELECT id
    FROM umbracoPropertyData
    WHERE propertyTypeId IN ({propertyTypeIdsInClause})
    AND textValue IS NOT NULL
    AND sortableValue IS NULL
    AND json_valid(textValue) = 1
    AND datetime(json_extract(textValue, '$.date')) IS NOT NULL
    LIMIT {BatchSize}
)";

        return ExecuteInBatches(database, sql, logger, "SQLite");
    }

    private static int ExecuteInBatches(IUmbracoDatabase database, string sql, ILogger logger, string databaseProviderName)
    {
        var totalRowsAffected = 0;
        while (true)
        {
            var rowsAffected = database.Execute(sql);
            if (rowsAffected <= 0)
            {
                break;
            }

            totalRowsAffected += rowsAffected;
            logger.LogInformation(
                "Populated sortableValue for batch of {BatchRowCount} property data rows ({TotalRowCount} total so far).",
                rowsAffected,
                totalRowsAffected);
        }

        logger.LogInformation(
            "Populated sortableValue for {RowCount} property data rows using {DatabaseFlavour} JSON functions.",
            totalRowsAffected,
            databaseProviderName);

        return totalRowsAffected;
    }
}
