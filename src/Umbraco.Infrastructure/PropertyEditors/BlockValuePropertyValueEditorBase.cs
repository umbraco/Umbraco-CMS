using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors;

internal abstract class BlockValuePropertyValueEditorBase : DataValueEditor, IDataValueReference, IDataValueTags
{
    private readonly IDataTypeConfigurationCache _dataTypeConfigurationCache;
    private readonly PropertyEditorCollection _propertyEditors;
    private readonly ILogger _logger;
    private readonly DataValueReferenceFactoryCollection _dataValueReferenceFactoryCollection;

    protected BlockValuePropertyValueEditorBase(
        DataEditorAttribute attribute,
        PropertyEditorCollection propertyEditors,
        IDataTypeConfigurationCache dataTypeConfigurationCache,
        ILocalizedTextService textService,
        ILogger logger,
        IShortStringHelper shortStringHelper,
        IJsonSerializer jsonSerializer,
        IIOHelper ioHelper,
        DataValueReferenceFactoryCollection dataValueReferenceFactoryCollection)
        : base(textService, shortStringHelper, jsonSerializer, ioHelper, attribute)
    {
        _propertyEditors = propertyEditors;
        _dataTypeConfigurationCache = dataTypeConfigurationCache;
        _logger = logger;
        _dataValueReferenceFactoryCollection = dataValueReferenceFactoryCollection;
    }

    /// <inheritdoc />
    public abstract IEnumerable<UmbracoEntityReference> GetReferences(object? value);

    protected IEnumerable<UmbracoEntityReference> GetBlockValueReferences(BlockValue blockValue)
    {
        var result = new HashSet<UmbracoEntityReference>();
        BlockItemData.BlockPropertyValue[] propertyValues = blockValue.ContentData.Concat(blockValue.SettingsData)
            .SelectMany(x => x.PropertyValues.Values).ToArray();
        foreach (IGrouping<string, object?> valuesByPropertyEditorAlias in propertyValues.GroupBy(x => x.PropertyType.PropertyEditorAlias, x => x.Value))
        {
            if (!_propertyEditors.TryGet(valuesByPropertyEditorAlias.Key, out IDataEditor? dataEditor))
            {
                continue;
            }

            var districtValues = valuesByPropertyEditorAlias.Distinct().ToArray();

            if (dataEditor.GetValueEditor() is IDataValueReference reference)
            {
                foreach (UmbracoEntityReference value in districtValues.SelectMany(reference.GetReferences))
                {
                    result.Add(value);
                }
            }

            IEnumerable<UmbracoEntityReference> references = _dataValueReferenceFactoryCollection.GetReferences(dataEditor, districtValues);

            foreach (UmbracoEntityReference value in references)
            {
                result.Add(value);
            }
        }

        return result;
    }

    /// <inheritdoc />
    public abstract IEnumerable<ITag> GetTags(object? value, object? dataTypeConfiguration, int? languageId);

    protected IEnumerable<ITag> GetBlockValueTags(BlockValue blockValue, int? languageId)
    {
        var result = new List<ITag>();

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

                object? configuration = _dataTypeConfigurationCache.GetConfiguration(prop.Value.PropertyType.DataTypeKey);

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

    protected void MapBlockValueToEditor(IProperty property, BlockValue blockValue)
    {
        MapBlockItemDataToEditor(property, blockValue.ContentData);
        MapBlockItemDataToEditor(property, blockValue.SettingsData);
    }

    private void MapBlockItemDataToEditor(IProperty property, List<BlockItemData> items)
    {
        var valEditors = new Dictionary<Guid, IDataValueEditor>();

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

                Guid dataTypeKey = prop.Value.PropertyType.DataTypeKey;
                if (!valEditors.TryGetValue(dataTypeKey, out IDataValueEditor? valEditor))
                {
                    var configuration = _dataTypeConfigurationCache.GetConfiguration(dataTypeKey);
                    valEditor = propEditor.GetValueEditor(configuration);

                    valEditors.Add(dataTypeKey, valEditor);
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
                var configuration = _dataTypeConfigurationCache.GetConfiguration(prop.Value.PropertyType.DataTypeKey);

                // Lookup the property editor
                IDataEditor? propEditor = _propertyEditors[prop.Value.PropertyType.PropertyEditorAlias];
                if (propEditor == null)
                {
                    continue;
                }

                // Create a fake content property data object
                var contentPropData = new ContentPropertyData(prop.Value.Value, configuration);

                // Get the property editor to do it's conversion
                var newValue = propEditor.GetValueEditor().FromEditor(contentPropData, prop.Value.Value);

                // update the raw value since this is what will get serialized out
                row.RawPropertyValues[prop.Key] = newValue;
            }
        }
    }
}
