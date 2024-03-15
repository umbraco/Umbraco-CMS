// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors;

internal abstract class BlockEditorPropertyValueEditor : BlockValuePropertyValueEditorBase
{
    private BlockEditorValues? _blockEditorValues;
    private readonly IDataTypeConfigurationCache _dataTypeConfigurationCache;
    private readonly PropertyEditorCollection _propertyEditors;
    private readonly DataValueReferenceFactoryCollection _dataValueReferenceFactories;
    private readonly ILogger<BlockEditorPropertyValueEditor> _logger;

    protected BlockEditorPropertyValueEditor(
        DataEditorAttribute attribute,
        PropertyEditorCollection propertyEditors,
        DataValueReferenceFactoryCollection dataValueReferenceFactories,
        IDataTypeConfigurationCache dataTypeConfigurationCache,
        ILocalizedTextService textService,
        ILogger<BlockEditorPropertyValueEditor> logger,
        IShortStringHelper shortStringHelper,
        IJsonSerializer jsonSerializer,
        IIOHelper ioHelper)
        : base(attribute, propertyEditors, dataTypeConfigurationCache, textService, logger, shortStringHelper, jsonSerializer, ioHelper, dataValueReferenceFactories)
    {
        _propertyEditors = propertyEditors;
        _dataValueReferenceFactories = dataValueReferenceFactories;
        _dataTypeConfigurationCache = dataTypeConfigurationCache;
        _logger = logger;
    }

    protected BlockEditorValues BlockEditorValues
    {
        get => _blockEditorValues ?? throw new NullReferenceException($"The property {nameof(BlockEditorValues)} must be initialized at value editor construction");
        set => _blockEditorValues = value;
    }

    /// <inheritdoc />
    public override IEnumerable<UmbracoEntityReference> GetReferences(object? value)
    {
        // Group by property editor alias to avoid duplicate lookups and optimize value parsing
        foreach (var valuesByPropertyEditorAlias in GetAllPropertyValues(value).GroupBy(x => x.PropertyType.PropertyEditorAlias, x => x.Value))
        {
            if (!_propertyEditors.TryGet(valuesByPropertyEditorAlias.Key, out IDataEditor? dataEditor))
            {
                continue;
            }

            // Use distinct values to avoid duplicate parsing of the same value
            foreach (UmbracoEntityReference reference in _dataValueReferenceFactories.GetReferences(dataEditor, valuesByPropertyEditorAlias.Distinct()))
            {
                yield return reference;
            }
        }
    }

    /// <inheritdoc />
    public override IEnumerable<ITag> GetTags(object? value, object? dataTypeConfiguration, int? languageId)
    {
        foreach (BlockItemData.BlockPropertyValue propertyValue in GetAllPropertyValues(value))
        {
            if (!_propertyEditors.TryGet(propertyValue.PropertyType.PropertyEditorAlias, out IDataEditor? dataEditor) ||
                dataEditor.GetValueEditor() is not IDataValueTags dataValueTags)
            {
                continue;
            }

            object? configuration = _dataTypeConfigurationCache.GetConfiguration(propertyValue.PropertyType.DataTypeKey);
            foreach (ITag tag in dataValueTags.GetTags(propertyValue.Value, configuration, languageId))
            {
                yield return tag;
            }
        }
    }

    private IEnumerable<BlockItemData.BlockPropertyValue> GetAllPropertyValues(object? value)
    {
        var rawJson = value == null ? string.Empty : value is string str ? str : value.ToString();

        BlockEditorData? blockEditorData = BlockEditorValues.DeserializeAndClean(rawJson);
        if (blockEditorData is null)
        {
            yield break;
        }

        // Return all property values from the content and settings data
        IEnumerable<BlockItemData> data = blockEditorData.BlockValue.ContentData.Concat(blockEditorData.BlockValue.SettingsData);
        foreach (BlockItemData.BlockPropertyValue propertyValue in data.SelectMany(x => x.PropertyValues.Select(x => x.Value)))
        {
            yield return propertyValue;
        }
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

        BlockEditorData? blockEditorData;
        try
        {
            blockEditorData = BlockEditorValues.DeserializeAndClean(val);
        }
        catch (JsonSerializationException)
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

        BlockEditorData? blockEditorData;
        try
        {
            blockEditorData = BlockEditorValues.DeserializeAndClean(editorValue.Value);
        }
        catch (JsonSerializationException)
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
        return JsonConvert.SerializeObject(blockEditorData.BlockValue, Formatting.None);
    }
}
