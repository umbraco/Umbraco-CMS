// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_22_0_0;

/// <summary>
///     Moves the data types using the multi node tree picker onto the dedicated document, media and member pickers.
/// </summary>
/// <remarks>
///     <para>
///         Only data types change. The dedicated pickers read the values the multi node tree picker stored, and store
///         them in their own format the next time they are saved, so property data is left as it is.
///     </para>
///     <para>
///         The data type rows are rewritten directly, as the multi node tree picker and its types no longer exist when
///         this runs.
///     </para>
/// </remarks>
// TODO (V22): register in UmbracoPlan when the multi node tree picker is removed.
internal sealed class MigrateMultiNodeTreePickerDataTypes : AsyncMigrationBase
{
    private const string MultiNodeTreePickerAlias = "Umbraco.MultiNodeTreePicker";

    private const string StartNodeKey = "startNode";
    private const string FilterKey = "filter";
    private const string ValidationLimitKey = "validationLimit";
    private const string IgnoreUserStartNodesKey = Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes;

    private static readonly Target[] _targets =
    [
        new(Mode.Content, Multiple: false, Constants.PropertyEditors.Aliases.ContentPicker, "Umb.PropertyEditorUi.DocumentPicker", ValueStorageType.Nvarchar),
        new(Mode.Content, Multiple: true, Constants.PropertyEditors.Aliases.MultipleDocumentPicker, "Umb.PropertyEditorUi.DocumentPicker.Multiple", ValueStorageType.Ntext),
        new(Mode.Media, Multiple: false, Constants.PropertyEditors.Aliases.SingleMediaPicker, "Umb.PropertyEditorUi.MediaPicker.Single", ValueStorageType.Ntext),
        new(Mode.Media, Multiple: true, Constants.PropertyEditors.Aliases.MediaPicker3, "Umb.PropertyEditorUi.MediaPicker", ValueStorageType.Ntext),
        new(Mode.Member, Multiple: false, Constants.PropertyEditors.Aliases.MemberPicker, "Umb.PropertyEditorUi.MemberPicker", ValueStorageType.Nvarchar),
        new(Mode.Member, Multiple: true, Constants.PropertyEditors.Aliases.MultipleMemberPicker, "Umb.PropertyEditorUi.MemberPicker.Multiple", ValueStorageType.Ntext),
    ];

    /// <summary>
    ///     Initializes a new instance of the <see cref="MigrateMultiNodeTreePickerDataTypes" /> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public MigrateMultiNodeTreePickerDataTypes(IMigrationContext context)
        : base(context)

        // The rewrite bypasses the data type service, so make sure all caches are rebuilt afterwards.
        => RebuildCache = true;

    private enum Mode
    {
        Content,
        Media,
        Member,
    }

    /// <inheritdoc />
    protected override Task MigrateAsync()
    {
        List<DataTypeDto> dataTypeDtos = Database.Fetch<DataTypeDto>(
            Database.SqlContext.Sql()
                .Select<DataTypeDto>()
                .From<DataTypeDto>()
                .Where<DataTypeDto>(x => x.EditorAlias == MultiNodeTreePickerAlias));

        var migrated = 0;
        foreach (DataTypeDto dataTypeDto in dataTypeDtos)
        {
            if (TryParseConfiguration(dataTypeDto, out JsonObject configuration) is false)
            {
                continue;
            }

            Target? target = GetTarget(configuration);
            if (target is null)
            {
                Logger.LogWarning(
                    "Data type {DataTypeId} uses the multi node tree picker with an unknown start node type {StartNodeType}, so it was not moved onto a dedicated picker.",
                    dataTypeDto.NodeId,
                    configuration[StartNodeKey]?["type"]?.ToJsonString());
                continue;
            }

            JsonObject targetConfiguration = MigrateConfiguration(configuration, target, dataTypeDto.NodeId);

            Database.Execute(Database.SqlContext.Sql()
                .Update<DataTypeDto>(u => u
                    .Set(x => x.EditorAlias, target.EditorAlias)
                    .Set(x => x.EditorUiAlias, target.EditorUiAlias)
                    .Set(x => x.DbType, target.StorageType.ToString())
                    .Set(x => x.Configuration, targetConfiguration.ToJsonString()))
                .Where<DataTypeDto>(x => x.NodeId == dataTypeDto.NodeId));

            Logger.LogInformation(
                "Moved data type {DataTypeId} from the multi node tree picker onto {EditorAlias}.",
                dataTypeDto.NodeId,
                target.EditorAlias);
            migrated++;
        }

        Logger.LogInformation(
            "Multi node tree picker migration complete: moved {Migrated} of {Total} data type(s) onto the dedicated pickers.",
            migrated,
            dataTypeDtos.Count);

        return Task.CompletedTask;
    }

    private bool TryParseConfiguration(DataTypeDto dataTypeDto, out JsonObject configuration)
    {
        configuration = [];
        if (string.IsNullOrWhiteSpace(dataTypeDto.Configuration))
        {
            return true;
        }

        try
        {
            if (JsonNode.Parse(dataTypeDto.Configuration) is JsonObject parsed)
            {
                configuration = parsed;
                return true;
            }
        }
        catch (JsonException exception)
        {
            Logger.LogWarning(
                exception,
                "Could not parse the configuration of multi node tree picker data type {DataTypeId}, so it was not moved onto a dedicated picker.",
                dataTypeDto.NodeId);
            return false;
        }

        Logger.LogWarning(
            "The configuration of multi node tree picker data type {DataTypeId} is not a JSON object, so it was not moved onto a dedicated picker.",
            dataTypeDto.NodeId);
        return false;
    }

    // The start node type selects the entity type, defaulting to documents, and a maximum of one item selects the
    // single picker.
    private static Target? GetTarget(JsonObject configuration)
    {
        Mode? mode = GetString(configuration[StartNodeKey]?["type"]) switch
        {
            null or "content" => Mode.Content,
            "media" => Mode.Media,
            "member" => Mode.Member,
            _ => null,
        };

        if (mode is null)
        {
            return null;
        }

        var multiple = GetMaximum(configuration) != 1;
        return _targets.Single(target => target.Mode == mode && target.Multiple == multiple);
    }

    private JsonObject MigrateConfiguration(JsonObject configuration, Target target, int dataTypeId)
    {
        var migrated = new JsonObject();
        JsonNode? startNode = configuration[StartNodeKey];

        // The single pickers have no item limits. Nothing is lost: a minimum only applies once items are picked, so
        // with a maximum of one it was already met by the one item.
        if (target.Multiple)
        {
            CopyIfPresent(configuration, migrated, ValidationLimitKey, ValidationLimitKey);
        }

        switch (target.Mode)
        {
            case Mode.Content:
                CopyIfPresent(configuration, migrated, IgnoreUserStartNodesKey, IgnoreUserStartNodesKey);
                CopyIfPresent(configuration, migrated, FilterKey, "allowedContentTypes");
                CopyIfPresent(startNode, migrated, "id", "startNodeId");
                CopyIfPresent(startNode, migrated, "dynamicRoot", "dynamicRoot");
                break;

            case Mode.Media:
                CopyIfPresent(configuration, migrated, IgnoreUserStartNodesKey, IgnoreUserStartNodesKey);
                CopyIfPresent(configuration, migrated, FilterKey, FilterKey);
                CopyIfPresent(startNode, migrated, "id", "startNodeId");
                if (IsPresent(startNode?["dynamicRoot"]))
                {
                    Logger.LogWarning(
                        "Data type {DataTypeId} picks media from a dynamic root, which the media pickers don't support, so it was dropped.",
                        dataTypeId);
                }

                break;

            case Mode.Member:
                CopyIfPresent(configuration, migrated, FilterKey, FilterKey);
                break;
        }

        if (IsPresent(startNode?["query"]))
        {
            Logger.LogWarning(
                "Data type {DataTypeId} has a start node query, which the dedicated pickers don't support, so it was dropped.",
                dataTypeId);
        }

        return migrated;
    }

    private static void CopyIfPresent(JsonNode? source, JsonObject destination, string sourceKey, string destinationKey)
    {
        JsonNode? value = (source as JsonObject)?[sourceKey];
        if (IsPresent(value))
        {
            destination[destinationKey] = value!.DeepClone();
        }
    }

    // Null and empty settings are treated as not set, so they aren't carried over.
    private static bool IsPresent(JsonNode? value)
        => value is not null && GetString(value) is not "";

    private static string? GetString(JsonNode? value)
        => value is JsonValue jsonValue && jsonValue.TryGetValue(out string? text) ? text : null;

    private static decimal? GetMaximum(JsonObject configuration)
        => configuration[ValidationLimitKey]?["max"] is JsonValue max && max.TryGetValue(out decimal value) ? value : null;

    private sealed record Target(Mode Mode, bool Multiple, string EditorAlias, string EditorUiAlias, ValueStorageType StorageType);
}
