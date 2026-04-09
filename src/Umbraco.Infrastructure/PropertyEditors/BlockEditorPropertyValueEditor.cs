// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Provides an abstract base class for property value editors based on block editors.
/// </summary>
public abstract class BlockEditorPropertyValueEditor<TValue, TLayout> : BlockValuePropertyValueEditorBase<TValue, TLayout>
    where TValue : BlockValue<TLayout>, new()
    where TLayout : class, IBlockLayoutItem, new()
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlockEditorPropertyValueEditor{TValue, TLayout}"/> class.
    /// </summary>
    protected BlockEditorPropertyValueEditor(
        PropertyEditorCollection propertyEditors,
        DataValueReferenceFactoryCollection dataValueReferenceFactories,
        IDataTypeConfigurationCache dataTypeConfigurationCache,
        IShortStringHelper shortStringHelper,
        IJsonSerializer jsonSerializer,
        BlockEditorVarianceHandler blockEditorVarianceHandler,
        ILanguageService languageService,
        IIOHelper ioHelper,
        DataEditorAttribute attribute,
        ILogger logger)
        : base(propertyEditors, dataTypeConfigurationCache, shortStringHelper, jsonSerializer, dataValueReferenceFactories, blockEditorVarianceHandler, languageService, ioHelper, attribute)
    {
        JsonSerializer = jsonSerializer;
        _logger = logger;
    }

    /// <summary>
    /// Gets the <see cref="IJsonSerializer"/>.
    /// </summary>
    protected IJsonSerializer JsonSerializer { get; }

    /// <inheritdoc />
    public override IEnumerable<UmbracoEntityReference> GetReferences(object? value)
    {
        TValue? blockValue = ParseBlockValue(value);
        return blockValue is not null
            ? GetBlockValueReferences(blockValue)
            : Enumerable.Empty<UmbracoEntityReference>();
    }

    /// <inheritdoc />
    public override IEnumerable<ITag> GetTags(object? value, object? dataTypeConfiguration, int? languageId)
    {
        TValue? blockValue = ParseBlockValue(value);
        return blockValue is not null
            ? GetBlockValueTags(blockValue, languageId)
            : Enumerable.Empty<ITag>();
    }

    private TValue? ParseBlockValue(object? value)
    {
        var rawJson = value == null ? string.Empty : value is string str ? str : value.ToString();
        return SafeParseBlockEditorData(rawJson)?.BlockValue;
    }

    /// <inheritdoc />
    public override object ToEditor(IProperty property, string? culture = null, string? segment = null)
    {
        var val = property.GetValue(culture, segment);

        BlockEditorData<TValue, TLayout>? blockEditorData = SafeParseBlockEditorData(val);

        if (blockEditorData == null)
        {
            return string.Empty;
        }

        MapBlockValueToEditor(property, blockEditorData.BlockValue, culture, segment);

        // return json convertable object
        return blockEditorData.BlockValue;
    }

    /// <inheritdoc />
    public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
    {
        // Note: we can't early return here if editorValue is null or empty, because these is the following case:
        // - current value not null (which means doc has at least one element in block list)
        // - editor value (new value) is null (which means doc has no elements in block list)
        // If we check editor value for null value and return before MapBlockValueFromEditor, then we will not trigger updates for properties.
        // For most of the properties this is fine, but for properties which contain other state it might be critical (e.g. file upload field).
        // So, we must run MapBlockValueFromEditor even if editorValue is null or string.IsNullOrWhiteSpace(editorValue.Value.ToString()) is true.

        BlockEditorData<TValue, TLayout>? currentBlockEditorData = SafeParseBlockEditorData(currentValue);
        BlockEditorData<TValue, TLayout>? blockEditorData = SafeParseBlockEditorData(editorValue.Value);

        CacheReferencedEntities(blockEditorData);

        // We can skip MapBlockValueFromEditor if both editorValue and currentValue values are empty.
        if (IsBlockEditorDataEmpty(currentBlockEditorData) && IsBlockEditorDataEmpty(blockEditorData))
        {
            return null;
        }

        MapBlockValueFromEditor(blockEditorData?.BlockValue, currentBlockEditorData?.BlockValue, editorValue.ContentKey);

        if (IsBlockEditorDataEmpty(blockEditorData))
        {
            return null;
        }

        SortBlockItemValuesByCulture(blockEditorData.BlockValue);
        return JsonSerializer.Serialize(blockEditorData.BlockValue);
    }

    private static bool IsBlockEditorDataEmpty([NotNullWhen(false)] BlockEditorData<TValue, TLayout>? editorData)
        => editorData is null || editorData.BlockValue.ContentData.Count == 0;

    // We don't throw on error here because we want to be able to parse what we can, even if some of the data is invalid. In cases where migrating
    // from nested content to blocks, we don't want to trigger a fatal error for retrieving references, as this isn't vital to the operation.
    // See: https://github.com/umbraco/Umbraco-CMS/issues/19784 and Umbraco support cases.
    private BlockEditorData<TValue, TLayout>? SafeParseBlockEditorData(object? value)
    {
        try
        {
            return BlockEditorValues.DeserializeAndClean(value);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(
                "Could not deserialize the provided property value into a block editor value: {PropertyValue}. Error: {ErrorMessage}.",
                value,
                ex.Message);
            return null;
        }
        catch
        {
            // If this occurs it means the data is invalid. It shouldn't happen could if we change the data format.
            return null;
        }
    }
}
