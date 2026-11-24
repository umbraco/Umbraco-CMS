// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_20_0_0;

/// <summary>
///     Converts the plain string <c>items</c> of the value list editors (Single Dropdown, Multiple Dropdown, Checkbox
///     List and Radio Button List) to option objects holding a <c>value</c> and a <c>label</c> that starts out as the value.
/// </summary>
/// <remarks>
///     Rewrites the stored data type configuration JSON directly, so it does not depend on the typed configuration.
/// </remarks>
internal sealed class MigrateValueListItemsToOptions : AsyncMigrationBase
{
    private static readonly string[] _editorAliases =
    [
        Constants.PropertyEditors.Aliases.SingleDropDown,
        Constants.PropertyEditors.Aliases.MultipleDropDown,
        Constants.PropertyEditors.Aliases.CheckBoxList,
        Constants.PropertyEditors.Aliases.RadioButtonList,
    ];

    /// <summary>
    /// Initializes a new instance of the <see cref="MigrateValueListItemsToOptions"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public MigrateValueListItemsToOptions(IMigrationContext context)
        : base(context)

        // The rewrite bypasses the data type service, so make sure all caches are rebuilt afterwards.
        => RebuildCache = true;

    /// <inheritdoc />
    protected override Task MigrateAsync()
    {
        List<DataTypeDto> dataTypeDtos = Database.Fetch<DataTypeDto>(
            Database.SqlContext.Sql()
                .Select<DataTypeDto>()
                .From<DataTypeDto>()
                .WhereIn<DataTypeDto>(x => x.EditorAlias, _editorAliases));

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
                    "Could not parse the configuration of data type {DataTypeId} for editor {EditorAlias}; skipping its options migration.",
                    dataTypeDto.NodeId,
                    dataTypeDto.EditorAlias);
                continue;
            }

            if (configuration is null)
            {
                unparseable++;
                Logger.LogWarning(
                    "The configuration of data type {DataTypeId} for editor {EditorAlias} is not a JSON object; skipping its options migration.",
                    dataTypeDto.NodeId,
                    dataTypeDto.EditorAlias);
                continue;
            }

#pragma warning disable CS0618 // Type or member is obsolete
            if (LegacyValueListItemsHelper.TryConvertPlainStringItems(configuration) is false)
#pragma warning restore CS0618 // Type or member is obsolete
            {
                unchanged++;
                continue;
            }

            Database.Execute(Database.SqlContext.Sql()
                .Update<DataTypeDto>(u => u.Set(x => x.Configuration, configuration.ToJsonString()))
                .Where<DataTypeDto>(x => x.NodeId == dataTypeDto.NodeId));

            migrated++;
        }

        Logger.LogInformation(
            "Value list options migration complete: migrated {Migrated} of {Total} data type(s) ({Unchanged} unchanged, {Unparseable} could not be parsed).",
            migrated,
            dataTypeDtos.Count,
            unchanged,
            unparseable);

        return Task.CompletedTask;
    }
}
