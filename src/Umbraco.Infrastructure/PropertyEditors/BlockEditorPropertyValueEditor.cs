// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics.CodeAnalysis;
using Umbraco.Cms.Core.Cache;
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
        DataEditorAttribute attribute)
        : base(propertyEditors, dataTypeConfigurationCache, shortStringHelper, jsonSerializer, dataValueReferenceFactories, blockEditorVarianceHandler, languageService, ioHelper, attribute) =>
        JsonSerializer = jsonSerializer;

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
        return BlockEditorValues.DeserializeAndClean(rawJson)?.BlockValue;
    }

    /// <inheritdoc />
    public override object ToEditor(IProperty property, string? culture = null, string? segment = null)
    {
        var val = property.GetValue(culture, segment);

        BlockEditorData<TValue, TLayout>? blockEditorData;
        try
        {
            blockEditorData = BlockEditorValues.DeserializeAndClean(val);
        }
        catch
        {
            // if this occurs it means the data is invalid, shouldn't happen but has happened if we change the data format.
            return string.Empty;
        }

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

        BlockEditorData<TValue, TLayout>? currentBlockEditorData = GetBlockEditorData(currentValue);
        BlockEditorData<TValue, TLayout>? blockEditorData = GetBlockEditorData(editorValue.Value);

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

        return JsonSerializer.Serialize(blockEditorData.BlockValue);
    }

    private BlockEditorData<TValue, TLayout>? GetBlockEditorData(object? value)
    {
        try
        {
            return BlockEditorValues.DeserializeAndClean(value);
        }
        catch
        {
            // If this occurs it means the data is invalid. It shouldn't happen could if we change the data format.
            return null;
        }
    }

    private static bool IsBlockEditorDataEmpty([NotNullWhen(false)] BlockEditorData<TValue, TLayout>? editorData)
        => editorData is null || editorData.BlockValue.ContentData.Count == 0;
}
