﻿using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors;

internal abstract class BlockValuePropertyValueEditorBase : DataValueEditor, IDataValueReference, IDataValueTags
{
    private readonly IDataTypeService _dataTypeService;
    private readonly PropertyEditorCollection _propertyEditors;
    private readonly ILogger _logger;

    protected BlockValuePropertyValueEditorBase(
        DataEditorAttribute attribute,
        PropertyEditorCollection propertyEditors,
        IDataTypeService dataTypeService,
        ILocalizedTextService textService,
        ILogger logger,
        IShortStringHelper shortStringHelper,
        IJsonSerializer jsonSerializer,
        IIOHelper ioHelper)
        : base(textService, shortStringHelper, jsonSerializer, ioHelper, attribute)
    {
        _propertyEditors = propertyEditors;
        _dataTypeService = dataTypeService;
        _logger = logger;
    }

    /// <inheritdoc />
    public abstract IEnumerable<UmbracoEntityReference> GetReferences(object? value);

    protected IEnumerable<UmbracoEntityReference> GetBlockValueReferences(BlockValue blockValue)
    {
        var result = new List<UmbracoEntityReference>();

        // loop through all content and settings data
        foreach (BlockItemData row in blockValue.ContentData.Concat(blockValue.SettingsData))
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

    /// <inheritdoc />
    public abstract IEnumerable<ITag> GetTags(object? value, object? dataTypeConfiguration, int? languageId);

    protected IEnumerable<ITag> GetBlockValueTags(BlockValue blockValue, int? languageId)
    {
        var result = new List<ITag>();
        IDictionary<int, IDataType?> dataTypes = new Dictionary<int, IDataType?>();

        // loop through all content and settings data
        foreach (BlockItemData row in blockValue.ContentData.Concat(blockValue.SettingsData))
        {
            foreach (KeyValuePair<string, BlockItemData.BlockPropertyValue> prop in row.PropertyValues)
            {
                IDataEditor? propEditor = _propertyEditors[prop.Value.PropertyType.PropertyEditorAlias];

                IDataValueEditor? valueEditor = propEditor?.GetValueEditor();
                if (valueEditor is not IDataValueTags tagsProvider)
                {
                    continue;
                }

                IDataType? dataType = null;
                if (dataTypes.ContainsKey(prop.Value.PropertyType.DataTypeId))
                {
                    dataType = dataTypes[prop.Value.PropertyType.DataTypeId];
                }
                else
                {
                    dataType = _dataTypeService.GetDataType(prop.Value.PropertyType.DataTypeId);
                    dataTypes.Add(prop.Value.PropertyType.DataTypeId, dataType);
                }
                var configuration = dataType?.Configuration;

                result.AddRange(tagsProvider.GetTags(prop.Value.Value, configuration, languageId));
            }
        }

        return result;
    }

    protected void MapBlockValueFromEditor(BlockValue blockValue)
    {
        MapBlockItemDataFromEditor(blockValue.ContentData);
        MapBlockItemDataFromEditor(blockValue.SettingsData);
    }

    protected void MapBlockValueToEditor(IProperty property, BlockValue blockValue, MapperContext? mapperContext = null)
    {
        MapBlockItemDataToEditor(property, blockValue.ContentData,mapperContext);
        MapBlockItemDataToEditor(property, blockValue.SettingsData,mapperContext);
    }

    private void MapBlockItemDataToEditor(IProperty property, List<BlockItemData> items, MapperContext? mapperContext = null)
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

                IDataType? dataType = null;
                if (mapperContext != null && mapperContext.Items.ContainsKey($"DataType-{prop.Value.PropertyType.DataTypeId}"))
                {
                    dataType = (mapperContext.Items[$"DataType-{prop.Value.PropertyType.DataTypeId}"] as IDataType);
                }
                else if(mapperContext!= null)
                {
                    dataType = _dataTypeService.GetDataType(prop.Value.PropertyType.DataTypeId);
                    mapperContext.Items.Add($"DataType-{prop.Value.PropertyType.DataTypeId}", dataType);
                }
                else
                {
                    dataType = _dataTypeService.GetDataType(prop.Value.PropertyType.DataTypeId);
                }

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

                var convValue = valEditor.ToEditor(tempProp, null, null, mapperContext);

                // update the raw value since this is what will get serialized out
                row.RawPropertyValues[prop.Key] = convValue;
            }
        }
    }

    private void MapBlockItemDataFromEditor(List<BlockItemData> items)
    {
        IDictionary<int, IDataType?> dataTypes = new Dictionary<int, IDataType?>();

        foreach (BlockItemData row in items)
        {
            foreach (KeyValuePair<string, BlockItemData.BlockPropertyValue> prop in row.PropertyValues)
            {
                // Fetch the property types prevalue
                IDataType? dataType = null;
                if (dataTypes.ContainsKey(prop.Value.PropertyType.DataTypeId))
                {
                    dataType = dataTypes[prop.Value.PropertyType.DataTypeId];
                }
                else
                {
                    dataType = _dataTypeService.GetDataType(prop.Value.PropertyType.DataTypeId);
                    dataTypes.Add(prop.Value.PropertyType.DataTypeId, dataType);
                }
                var propConfiguration = dataType?.Configuration;

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
}
