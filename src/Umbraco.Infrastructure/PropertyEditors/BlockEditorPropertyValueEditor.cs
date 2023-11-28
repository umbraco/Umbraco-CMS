// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors;

internal abstract class BlockEditorPropertyValueEditor<TValue, TLayout> : BlockValuePropertyValueEditorBase<TValue, TLayout>
    where TValue : BlockValue<TLayout>, new()
    where TLayout : class, IBlockLayoutItem, new()
{
    private readonly IJsonSerializer _jsonSerializer;
    private BlockEditorValues<TValue, TLayout>? _blockEditorValues;

    protected BlockEditorPropertyValueEditor(
        DataEditorAttribute attribute,
        PropertyEditorCollection propertyEditors,
        IDataTypeService dataTypeService,
        ILocalizedTextService textService,
        ILogger<BlockEditorPropertyValueEditor<TValue, TLayout>> logger,
        IShortStringHelper shortStringHelper,
        IJsonSerializer jsonSerializer,
        IIOHelper ioHelper)
        : base(attribute, propertyEditors, dataTypeService, textService, logger, shortStringHelper, jsonSerializer, ioHelper)
        => _jsonSerializer = jsonSerializer;

    protected BlockEditorValues<TValue, TLayout> BlockEditorValues
    {
        get => _blockEditorValues ?? throw new NullReferenceException($"The property {nameof(BlockEditorValues)} must be initialized at value editor construction");
        set => _blockEditorValues = value;
    }

    /// <inheritdoc />
    public override IEnumerable<UmbracoEntityReference> GetReferences(object? value)
    {
        var rawJson = value == null ? string.Empty : value is string str ? str : value.ToString();

        BlockEditorData<TValue, TLayout>? blockEditorData = BlockEditorValues.DeserializeAndClean(rawJson);
        if (blockEditorData == null)
        {
            return Enumerable.Empty<UmbracoEntityReference>();
        }

        return GetBlockValueReferences(blockEditorData.BlockValue);
    }

    /// <inheritdoc />
    public override IEnumerable<ITag> GetTags(object? value, object? dataTypeConfiguration, int? languageId)
    {
        var rawJson = value == null ? string.Empty : value is string str ? str : value.ToString();

        BlockEditorData<TValue, TLayout>? blockEditorData = BlockEditorValues.DeserializeAndClean(rawJson);
        if (blockEditorData == null)
        {
            return Enumerable.Empty<ITag>();
        }

        return GetBlockValueTags(blockEditorData.BlockValue, languageId);
    }

    // note: there is NO variant support here

    /// <summary>
    ///     Ensure that sub-editor values are translated through their ToEditor methods
    /// </summary>
    /// <param name="property"></param>
    /// <param name="culture"></param>
    /// <param name="segment"></param>
    /// <returns></returns>
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

        MapBlockValueToEditor(property, blockEditorData.BlockValue);

        // return json convertable object
        return blockEditorData.BlockValue;
    }

    /// <summary>
    ///     Ensure that sub-editor values are translated through their FromEditor methods
    /// </summary>
    /// <param name="editorValue"></param>
    /// <param name="currentValue"></param>
    /// <returns></returns>
    public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
    {
        if (editorValue.Value == null || string.IsNullOrWhiteSpace(editorValue.Value.ToString()))
        {
            return null;
        }

        BlockEditorData<TValue, TLayout>? blockEditorData;
        try
        {
            blockEditorData = BlockEditorValues.DeserializeAndClean(editorValue.Value);
        }
        catch
        {
            // if this occurs it means the data is invalid, shouldn't happen but has happened if we change the data format.
            return string.Empty;
        }

        if (blockEditorData == null || blockEditorData.BlockValue.ContentData.Count == 0)
        {
            return string.Empty;
        }

        MapBlockValueFromEditor(blockEditorData.BlockValue);

        // return json
        return _jsonSerializer.Serialize(blockEditorData.BlockValue);
    }
}
