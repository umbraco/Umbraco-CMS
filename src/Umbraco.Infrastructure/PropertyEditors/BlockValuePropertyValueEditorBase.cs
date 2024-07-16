using Microsoft.Extensions.Logging;
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

internal abstract class BlockValuePropertyValueEditorBase<TValue, TLayout> : DataValueEditor, IDataValueReference, IDataValueTags
    where TValue : BlockValue<TLayout>, new()
    where TLayout : class, IBlockLayoutItem, new()
{
    private readonly IDataTypeConfigurationCache _dataTypeConfigurationCache;
    private readonly PropertyEditorCollection _propertyEditors;
    private readonly ILogger _logger;
    private readonly DataValueReferenceFactoryCollection _dataValueReferenceFactoryCollection;
    private readonly BlockEditorVarianceHandler _blockEditorVarianceHandler;

    protected BlockValuePropertyValueEditorBase(
        DataEditorAttribute attribute,
        PropertyEditorCollection propertyEditors,
        IDataTypeConfigurationCache dataTypeConfigurationCache,
        ILocalizedTextService textService,
        ILogger logger,
        IShortStringHelper shortStringHelper,
        IJsonSerializer jsonSerializer,
        IIOHelper ioHelper,
        DataValueReferenceFactoryCollection dataValueReferenceFactoryCollection,
        BlockEditorVarianceHandler blockEditorVarianceHandler)
        : base(textService, shortStringHelper, jsonSerializer, ioHelper, attribute)
    {
        _propertyEditors = propertyEditors;
        _dataTypeConfigurationCache = dataTypeConfigurationCache;
        _logger = logger;
        _dataValueReferenceFactoryCollection = dataValueReferenceFactoryCollection;
        _blockEditorVarianceHandler = blockEditorVarianceHandler;
    }

    /// <inheritdoc />
    public abstract IEnumerable<UmbracoEntityReference> GetReferences(object? value);

    protected IEnumerable<UmbracoEntityReference> GetBlockValueReferences(TValue blockValue)
    {
        var result = new HashSet<UmbracoEntityReference>();
        BlockPropertyValue[] blockPropertyValues = blockValue.ContentData.Concat(blockValue.SettingsData)
            .SelectMany(x => x.Properties).ToArray();
        if (blockPropertyValues.Any(p => p.PropertyType is null))
        {
            throw new ArgumentException("One or more block properties did not have a resolved property type. Block editor values must be resolved before attempting to find references within them.", nameof(blockValue));
        }

        foreach (IGrouping<string, object?> valuesByPropertyEditorAlias in blockPropertyValues.GroupBy(x => x.PropertyType!.PropertyEditorAlias, x => x.Value))
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

    protected IEnumerable<ITag> GetBlockValueTags(TValue blockValue, int? languageId)
    {
        var result = new List<ITag>();

        // loop through all content and settings data
        foreach (BlockItemData row in blockValue.ContentData.Concat(blockValue.SettingsData))
        {
            foreach (BlockPropertyValue blockPropertyValue in row.Properties)
            {
                if (blockPropertyValue.PropertyType is null)
                {
                    throw new ArgumentException("One or more block properties did not have a resolved property type. Block editor values must be resolved before attempting to find tags within them.", nameof(blockValue));
                }

                IDataEditor? propEditor = _propertyEditors[blockPropertyValue.PropertyType.PropertyEditorAlias];

                IDataValueEditor? valueEditor = propEditor?.GetValueEditor();
                if (valueEditor is not IDataValueTags tagsProvider)
                {
                    continue;
                }

                object? configuration = _dataTypeConfigurationCache.GetConfiguration(blockPropertyValue.PropertyType.DataTypeKey);

                result.AddRange(tagsProvider.GetTags(blockPropertyValue.Value, configuration, languageId));
            }
        }

        return result;
    }

    protected void MapBlockValueFromEditor(TValue blockValue)
    {
        MapBlockItemDataFromEditor(blockValue.ContentData);
        MapBlockItemDataFromEditor(blockValue.SettingsData);
    }

    protected void MapBlockValueToEditor(IProperty property, TValue blockValue, string? culture, string? segment)
    {
        MapBlockItemDataToEditor(property, blockValue.ContentData, culture, segment);
        MapBlockItemDataToEditor(property, blockValue.SettingsData, culture, segment);
    }

    private void MapBlockItemDataToEditor(IProperty property, List<BlockItemData> items, string? culture, string? segment)
    {
        var valueEditorsByKey = new Dictionary<Guid, IDataValueEditor>();

        foreach (BlockItemData item in items)
        {
            foreach (BlockPropertyValue blockPropertyValue in item.Properties)
            {
                IPropertyType? propertyType = blockPropertyValue.PropertyType;
                if (propertyType is null)
                {
                    throw new ArgumentException("One or more block properties did not have a resolved property type. Block editor values must be resolved before attempting to map them to editor.", nameof(items));
                }

                IDataEditor? propertyEditor = _propertyEditors[propertyType.PropertyEditorAlias];
                if (propertyEditor is null)
                {
                    // leave the current block property value as-is - will be used to render a fallback output in the client
                    continue;
                }

                // if changes were made to the element type variation, we need those changes reflected in the block property values.
                // for regular content this happens when a content type is saved (copies of property values are created in the DB),
                // but for local block level properties we don't have that kind of handling, so we to do it manually.
                // to be friendly we'll map "formerly invariant properties" to the default language ISO code instead of performing a
                // hard reset of the property values (which would likely be the most correct thing to do from a data point of view).
                _blockEditorVarianceHandler.AlignPropertyVariance(blockPropertyValue, propertyType, culture, segment).GetAwaiter().GetResult();

                if (!valueEditorsByKey.TryGetValue(propertyType.DataTypeKey, out IDataValueEditor? valueEditor))
                {
                    var configuration = _dataTypeConfigurationCache.GetConfiguration(propertyType.DataTypeKey);
                    valueEditor = propertyEditor.GetValueEditor(configuration);

                    valueEditorsByKey.Add(propertyType.DataTypeKey, valueEditor);
                }

                var tempProp = new Property(propertyType);
                tempProp.SetValue(blockPropertyValue.Value, blockPropertyValue.Culture, blockPropertyValue.Segment);

                var editorValue = valueEditor.ToEditor(tempProp, blockPropertyValue.Culture, blockPropertyValue.Segment);

                // update the raw value since this is what will get serialized out
                blockPropertyValue.Value = editorValue;
            }
        }
    }

    private void MapBlockItemDataFromEditor(List<BlockItemData> items)
    {
        foreach (BlockItemData item in items)
        {
            foreach (BlockPropertyValue blockPropertyValue in item.Properties)
            {
                IPropertyType? propertyType = blockPropertyValue.PropertyType;
                if (propertyType is null)
                {
                    throw new ArgumentException("One or more block properties did not have a resolved property type. Block editor values must be resolved before attempting to map them from editor.", nameof(items));
                }

                // Lookup the property editor
                IDataEditor? propertyEditor = _propertyEditors[propertyType.PropertyEditorAlias];
                if (propertyEditor is null)
                {
                    continue;
                }

                // Fetch the property types prevalue
                var configuration = _dataTypeConfigurationCache.GetConfiguration(propertyType.DataTypeKey);

                // Create a fake content property data object
                var propertyData = new ContentPropertyData(blockPropertyValue.Value, configuration);

                // Get the property editor to do it's conversion
                var newValue = propertyEditor.GetValueEditor().FromEditor(propertyData, blockPropertyValue.Value);

                // update the raw value since this is what will get serialized out
                blockPropertyValue.Value = newValue;
            }
        }
    }
}
