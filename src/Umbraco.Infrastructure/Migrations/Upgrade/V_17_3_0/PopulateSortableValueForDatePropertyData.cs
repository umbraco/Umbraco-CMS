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
        // Build the IN clause for the property editor aliases.
        var aliasesInClause = string.Join(", ", _dateTimePropertyEditorAliases.Select(a => $"'{a}'"));

        return databaseType == DatabaseType.SQLite
            ? MigrateSQLite(database, aliasesInClause, logger)
            : MigrateSqlServer(database, aliasesInClause, logger);
    }

    /// <summary>
    /// SQL Server migration using JSON_VALUE and datetimeoffset for proper UTC conversion.
    /// </summary>
    /// <remarks>
    /// The JSON format is: {"date":"2025-11-05T15:31:00+00:00","timeZone":"UTC"}
    /// We extract the date, parse it as datetimeoffset, convert to UTC, and format as ISO 8601.
    /// The resulting format is: 2025-11-05T15:31:00.0000000+00:00
    /// </remarks>
    private static int MigrateSqlServer(IUmbracoDatabase database, string aliasesInClause, ILogger logger)
    {
        // SQL Server: Use ISJSON to validate JSON before parsing, then use JSON_VALUE to extract the date,
        // CAST to datetimeoffset, SWITCHOFFSET to convert to UTC, and CONVERT with style 127 for ISO 8601 format.
        // Style 127 produces: yyyy-MM-ddTHH:mm:ss.nnnnnnn or yyyy-MM-ddTHH:mm:ss.nnnnnnn+00:00
        var sql = $@"
UPDATE umbracoPropertyData
SET sortableValue = CONVERT(varchar(50), SWITCHOFFSET(CAST(JSON_VALUE(textValue, '$.date') AS datetimeoffset), '+00:00'), 127)
WHERE propertyTypeId IN (
    SELECT id
    FROM cmsPropertyType
    WHERE dataTypeId IN (
        SELECT nodeId
        FROM umbracoDataType
        WHERE propertyEditorAlias IN ({aliasesInClause})
    )
)
AND textValue IS NOT NULL
AND sortableValue IS NULL
AND ISJSON(textValue) = 1
AND JSON_VALUE(textValue, '$.date') IS NOT NULL";

        var rowsAffected = database.Execute(sql);
        logger.LogInformation(
            "Populated sortableValue for {RowCount} property data rows using SQL Server JSON functions.",
            rowsAffected);

        return rowsAffected;
    }

    /// <summary>
    /// SQLite migration using json_extract for date extraction.
    /// </summary>
    /// <remarks>
    /// SQLite has limited datetime manipulation capabilities, so we extract the date string
    /// and use strftime to normalize it. For dates with timezone offsets, SQLite's datetime
    /// function can parse ISO 8601 format and converts to UTC.
    /// The resulting format is: yyyy-MM-ddTHH:mm:ssZ
    /// </remarks>
    private static int MigrateSQLite(IUmbracoDatabase database, string aliasesInClause, ILogger logger)
    {
        // SQLite: Use json_valid to validate JSON before parsing, then use json_extract to get the date value,
        // datetime() to parse and normalize to UTC. The datetime() function automatically handles timezone
        // offsets in ISO 8601 format and converts to UTC.
        // We then format using strftime to get a consistent sortable format.
        var sql = $@"
UPDATE umbracoPropertyData
SET sortableValue = strftime('%Y-%m-%dT%H:%M:%SZ', datetime(json_extract(textValue, '$.date')))
WHERE propertyTypeId IN (
    SELECT id
    FROM cmsPropertyType
    WHERE dataTypeId IN (
        SELECT nodeId
        FROM umbracoDataType
        WHERE propertyEditorAlias IN ({aliasesInClause})
    )
)
AND textValue IS NOT NULL
AND sortableValue IS NULL
AND json_valid(textValue) = 1
AND json_extract(textValue, '$.date') IS NOT NULL";

        var rowsAffected = database.Execute(sql);
        logger.LogInformation(
            "Populated sortableValue for {RowCount} property data rows using SQLite JSON functions.",
            rowsAffected);

        return rowsAffected;
    }
}
