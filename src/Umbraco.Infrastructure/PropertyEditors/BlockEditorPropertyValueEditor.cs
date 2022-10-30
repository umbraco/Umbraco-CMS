// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors;

internal abstract class BlockEditorPropertyValueEditor : DataValueEditor, IDataValueReference
{
    private BlockEditorValues? _blockEditorValues;
    private readonly IDataTypeService _dataTypeService;
    private readonly ILogger<BlockEditorPropertyValueEditor> _logger;
    private readonly PropertyEditorCollection _propertyEditors;

    protected BlockEditorPropertyValueEditor(
        DataEditorAttribute attribute,
        PropertyEditorCollection propertyEditors,
        IDataTypeService dataTypeService,
        ILocalizedTextService textService,
        ILogger<BlockEditorPropertyValueEditor> logger,
        IShortStringHelper shortStringHelper,
        IJsonSerializer jsonSerializer,
        IIOHelper ioHelper)
        : base(textService, shortStringHelper, jsonSerializer, ioHelper, attribute)
    {
        _propertyEditors = propertyEditors;
        _dataTypeService = dataTypeService;
        _logger = logger;
    }

    protected BlockEditorValues BlockEditorValues
    {
        get => _blockEditorValues ?? throw new NullReferenceException($"The property {nameof(BlockEditorValues)} must be initialized at value editor construction");
        set => _blockEditorValues = value;
    }

    public IEnumerable<UmbracoEntityReference> GetReferences(object? value)
    {
        var rawJson = value == null ? string.Empty : value is string str ? str : value.ToString();

        var result = new List<UmbracoEntityReference>();
        BlockEditorData? blockEditorData = BlockEditorValues.DeserializeAndClean(rawJson);
        if (blockEditorData == null)
        {
            return Enumerable.Empty<UmbracoEntityReference>();
        }

        // loop through all content and settings data
        foreach (BlockItemData row in blockEditorData.BlockValue.ContentData.Concat(blockEditorData.BlockValue.SettingsData))
        {
            foreach (KeyValuePair<string, BlockItemData.BlockPropertyValue> prop in row.PropertyValues)
            {
                IDataEditor? propEditor = _propertyEditors[prop.Value.PropertyType.PropertyEditorAlias];

                IDataValueEditor? valueEditor = propEditor?.GetValueEditor();
                if (!(valueEditor is IDataValueReference reference))
                {
                    continue;
                }

                var val = prop.Value.Value?.ToString();

                IEnumerable<UmbracoEntityReference> refs = reference.GetReferences(val);

                result.AddRange(refs);
            }
        }

        return result;
    }

    #region Convert database // editor

    // note: there is NO variant support here

    /// <summary>
    ///     Ensure that sub-editor values are translated through their ToEditor methods
    /// </summary>
    /// <param name="property"></param>
    /// <param name="dataTypeService"></param>
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

        MapBlockItemDataToEditor(property, blockEditorData.BlockValue.ContentData);
        MapBlockItemDataToEditor(property, blockEditorData.BlockValue.SettingsData);

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

        MapBlockItemDataFromEditor(blockEditorData.BlockValue.ContentData);
        MapBlockItemDataFromEditor(blockEditorData.BlockValue.SettingsData);

        // return json
        return JsonConvert.SerializeObject(blockEditorData.BlockValue, Formatting.None);
    }

    private void MapBlockItemDataToEditor(IProperty property, List<BlockItemData> items)
    {
        var valEditors = new Dictionary<int, IDataValueEditor>();

        foreach (BlockItemData row in items)
        {
            foreach (KeyValuePair<string, BlockItemData.BlockPropertyValue> prop in row.PropertyValues)
            {
                // create a temp property with the value
                // - force it to be culture invariant as the block editor can't handle culture variant element properties
                prop.Value.PropertyType.Variations = ContentVariation.Nothing;
                var tempProp = new Property(prop.Value.PropertyType);
                tempProp.SetValue(prop.Value.Value);

                IDataEditor? propEditor = _propertyEditors[prop.Value.PropertyType.PropertyEditorAlias];
                if (propEditor == null)
                {
                    // NOTE: This logic was borrowed from Nested Content and I'm unsure why it exists.
                    // if the property editor doesn't exist I think everything will break anyways?
                    // update the raw value since this is what will get serialized out
                    row.RawPropertyValues[prop.Key] = tempProp.GetValue()?.ToString();
                    continue;
                }

                IDataType? dataType = _dataTypeService.GetDataType(prop.Value.PropertyType.DataTypeId);
                if (dataType == null)
                {
                    // deal with weird situations by ignoring them (no comment)
                    row.PropertyValues.Remove(prop.Key);
                    _logger.LogWarning(
                        "ToEditor removed property value {PropertyKey} in row {RowId} for property type {PropertyTypeAlias}",
                        prop.Key,
                        row.Key,
                        property.PropertyType.Alias);
                    continue;
                }

                if (!valEditors.TryGetValue(dataType.Id, out IDataValueEditor? valEditor))
                {
                    var tempConfig = dataType.Configuration;
                    valEditor = propEditor.GetValueEditor(tempConfig);

                    valEditors.Add(dataType.Id, valEditor);
                }

                var convValue = valEditor.ToEditor(tempProp);

                // update the raw value since this is what will get serialized out
                row.RawPropertyValues[prop.Key] = convValue;
            }
        }
    }

    private void MapBlockItemDataFromEditor(List<BlockItemData> items)
    {
        foreach (BlockItemData row in items)
        {
            foreach (KeyValuePair<string, BlockItemData.BlockPropertyValue> prop in row.PropertyValues)
            {
                // Fetch the property types prevalue
                var propConfiguration = _dataTypeService.GetDataType(prop.Value.PropertyType.DataTypeId)?.Configuration;

                // Lookup the property editor
                IDataEditor? propEditor = _propertyEditors[prop.Value.PropertyType.PropertyEditorAlias];
                if (propEditor == null)
                {
                    continue;
                }

                // Create a fake content property data object
                var contentPropData = new ContentPropertyData(prop.Value.Value, propConfiguration);

                // Get the property editor to do it's conversion
                var newValue = propEditor.GetValueEditor().FromEditor(contentPropData, prop.Value.Value);

                // update the raw value since this is what will get serialized out
                row.RawPropertyValues[prop.Key] = newValue;
            }
        }
    }

    #endregion
}
