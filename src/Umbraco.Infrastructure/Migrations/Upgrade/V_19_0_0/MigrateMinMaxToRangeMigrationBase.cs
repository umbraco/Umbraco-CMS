// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
///     Base migration that converts a property editor's separate minimum/maximum configuration fields into a single
///     range object. Rewrites the stored data type configuration JSON directly (bypassing the data type service,
///     whose new range validation would otherwise reject legacy configurations where the minimum exceeds the maximum).
/// </summary>
internal abstract class MigrateMinMaxToRangeMigrationBase : AsyncMigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MigrateMinMaxToRangeMigrationBase"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    protected MigrateMinMaxToRangeMigrationBase(IMigrationContext context)
        : base(context)

        // The rewrite bypasses the data type service, so make sure all caches are rebuilt afterwards.
        => RebuildCache = true;

    /// <summary>
    ///     Gets the property editor alias of the data types to migrate.
    /// </summary>
    protected abstract string EditorAlias { get; }

    /// <summary>
    ///     Migrates a single data type's configuration in place.
    /// </summary>
    /// <returns><c>true</c> when the configuration was changed and should be persisted.</returns>
    protected abstract bool TryMigrateConfiguration(JsonObject configuration);

    /// <inheritdoc />
    protected override Task MigrateAsync()
    {
        List<DataTypeDto> dataTypeDtos = Database.Fetch<DataTypeDto>(
            Database.SqlContext.Sql()
                .Select<DataTypeDto>()
                .From<DataTypeDto>()
                .Where<DataTypeDto>(x => x.EditorAlias == EditorAlias));

        var migrated = 0;
        var unchanged = 0;
        var unparseable = 0;

        foreach (DataTypeDto dataTypeDto in dataTypeDtos)
        {
            if (string.IsNullOrWhiteSpace(dataTypeDto.Configuration))
            {
                unchanged++;
                continue;
            }

            JsonObject? configuration;
            try
            {
                configuration = JsonNode.Parse(dataTypeDto.Configuration) as JsonObject;
            }
            catch (JsonException exception)
            {
                unparseable++;
                Logger.LogWarning(
                    exception,
                    "Could not parse the configuration of data type {DataTypeId} for editor {EditorAlias}; skipping its range migration.",
                    dataTypeDto.NodeId,
                    EditorAlias);
                continue;
            }

            if (configuration is null)
            {
                unparseable++;
                Logger.LogWarning(
                    "The configuration of data type {DataTypeId} for editor {EditorAlias} is not a JSON object; skipping its range migration.",
                    dataTypeDto.NodeId,
                    EditorAlias);
                continue;
            }

            if (TryMigrateConfiguration(configuration) is false)
            {
                unchanged++;
                Logger.LogDebug(
                    "No range configuration to migrate for data type {DataTypeId} of editor {EditorAlias}.",
                    dataTypeDto.NodeId,
                    EditorAlias);
                continue;
            }

            Database.Execute(
                $"UPDATE {DataTypeDto.TableName} SET config = @0 WHERE {DataTypeDto.PrimaryKeyColumnName} = @1",
                configuration.ToJsonString(),
                dataTypeDto.NodeId);
            migrated++;
        }

        Logger.LogInformation(
            "Range configuration migration for editor {EditorAlias} complete: migrated {Migrated} of {Total} data type(s) ({Unchanged} unchanged, {Unparseable} could not be parsed).",
            EditorAlias,
            migrated,
            dataTypeDtos.Count,
            unchanged,
            unparseable);

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Converts two separate top-level min/max keys into a single range object under <paramref name="rangeKey" />.
    /// </summary>
    /// <param name="configuration">The configuration object to mutate.</param>
    /// <param name="minKey">The existing minimum key.</param>
    /// <param name="maxKey">The existing maximum key.</param>
    /// <param name="rangeKey">The new range key.</param>
    /// <param name="minZeroIsUnbounded">Whether a stored minimum of zero represents "no minimum".</param>
    /// <param name="maxZeroIsUnbounded">Whether a stored maximum of zero represents "no maximum".</param>
    /// <returns><c>true</c> when a change was made.</returns>
    internal static bool MigrateTopLevelRange(
        JsonObject configuration,
        string minKey,
        string maxKey,
        string rangeKey,
        bool minZeroIsUnbounded,
        bool maxZeroIsUnbounded)
    {
        var minName = FindKey(configuration, minKey);
        var maxName = FindKey(configuration, maxKey);

        if (minName is null && maxName is null)
        {
            // Nothing to migrate (e.g. already migrated).
            return false;
        }

        decimal? min = minName is not null ? ToBound(configuration[minName], minZeroIsUnbounded) : null;
        decimal? max = maxName is not null ? ToBound(configuration[maxName], maxZeroIsUnbounded) : null;

        if (minName is not null)
        {
            configuration.Remove(minName);
        }

        if (maxName is not null)
        {
            configuration.Remove(maxName);
        }

        SetRange(configuration, rangeKey, min, max);
        return true;
    }

    /// <summary>
    ///     Sets a range object with the supplied bounds, omitting it entirely when both bounds are absent.
    /// </summary>
    internal static void SetRange(JsonObject configuration, string rangeKey, decimal? min, decimal? max)
    {
        var existingKey = FindKey(configuration, rangeKey);
        if (existingKey is not null)
        {
            configuration.Remove(existingKey);
        }

        if (min is null && max is null)
        {
            return;
        }

        var range = new JsonObject();
        if (min is not null)
        {
            range["min"] = JsonValue.Create(min.Value);
        }

        if (max is not null)
        {
            range["max"] = JsonValue.Create(max.Value);
        }

        configuration[rangeKey] = range;
    }

    /// <summary>
    ///     Parses a JSON node into a numeric bound, treating zero as unbounded when requested.
    /// </summary>
    internal static decimal? ToBound(JsonNode? node, bool zeroIsUnbounded)
    {
        decimal? value = ToDecimal(node);
        if (value is null)
        {
            return null;
        }

        return zeroIsUnbounded && value.Value == 0m ? null : value;
    }

    private static decimal? ToDecimal(JsonNode? node)
    {
        if (node is null)
        {
            return null;
        }

        try
        {
            return node.GetValue<decimal>();
        }
        catch
        {
            return decimal.TryParse(node.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
        }
    }

    /// <summary>
    ///     Finds the actual property name in the configuration that matches <paramref name="key" /> case-insensitively.
    /// </summary>
    /// <param name="configuration">The configuration object to search.</param>
    /// <param name="key">The key to look for.</param>
    /// <returns>The matching property name as stored, or <c>null</c> when not present.</returns>
    internal static string? FindKey(JsonObject configuration, string key)
        => configuration.Select(pair => pair.Key)
            .FirstOrDefault(existing => existing.InvariantEquals(key));
}
