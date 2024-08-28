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

public abstract class BlockValuePropertyValueEditorBase<TValue, TLayout> : DataValueEditor, IDataValueReference, IDataValueTags
    where TValue : BlockValue<TLayout>, new()
    where TLayout : class, IBlockLayoutItem, new()
{
    private readonly IDataTypeConfigurationCache _dataTypeConfigurationCache;
    private readonly PropertyEditorCollection _propertyEditors;
    private readonly ILogger _logger;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly DataValueReferenceFactoryCollection _dataValueReferenceFactoryCollection;
    private readonly BlockEditorVarianceHandler _blockEditorVarianceHandler;
    private BlockEditorValues<TValue, TLayout>? _blockEditorValues;

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
        _jsonSerializer = jsonSerializer;
        _dataValueReferenceFactoryCollection = dataValueReferenceFactoryCollection;
        _blockEditorVarianceHandler = blockEditorVarianceHandler;
    }

    /// <inheritdoc />
    public abstract IEnumerable<UmbracoEntityReference> GetReferences(object? value);

    protected abstract TValue CreateWithLayout(IEnumerable<TLayout> layout);

    protected BlockEditorValues<TValue, TLayout> BlockEditorValues
    {
        get => _blockEditorValues ?? throw new NullReferenceException($"The property {nameof(BlockEditorValues)} must be initialized at value editor construction");
        set => _blockEditorValues = value;
    }

    protected IEnumerable<UmbracoEntityReference> GetBlockValueReferences(TValue blockValue)
    {
        var result = new HashSet<UmbracoEntityReference>();
        BlockPropertyValue[] blockPropertyValues = blockValue.ContentData.Concat(blockValue.SettingsData)
            .SelectMany(x => x.Values).ToArray();
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
            foreach (BlockPropertyValue blockPropertyValue in row.Values)
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

    protected IEnumerable<Guid> ConfiguredElementTypeKeys(IBlockConfiguration configuration)
    {
        yield return configuration.ContentElementTypeKey;
        if (configuration.SettingsElementTypeKey is not null)
        {
            yield return configuration.SettingsElementTypeKey.Value;
        }
    }

    private void MapBlockItemDataToEditor(IProperty property, List<BlockItemData> items, string? culture, string? segment)
    {
        var valueEditorsByKey = new Dictionary<Guid, IDataValueEditor>();

        foreach (BlockItemData item in items)
        {
            foreach (BlockPropertyValue blockPropertyValue in item.Values)
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
            foreach (BlockPropertyValue blockPropertyValue in item.Values)
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

    internal object? PublishPartialValueForCulture(object? editedValue, object? publishedValue, string? culture)
    {
        if (editedValue is null)
        {
            return null;
        }

        // parse the edited value as block editor data
        BlockEditorData<TValue, TLayout>? editedBlockEditorValues = BlockEditorValues.DeserializeAndClean(editedValue);
        if (editedBlockEditorValues?.Layout is null)
        {
            return null;
        }

        // parse the published value as block editor data (fallback to an empty set of block editor data)
        BlockEditorData<TValue, TLayout> publishedBlockEditorValues =
            (publishedValue is not null ? BlockEditorValues.DeserializeAndClean(publishedValue) : null)
            ?? new BlockEditorData<TValue, TLayout>([], CreateWithLayout(editedBlockEditorValues.Layout));

        PublishPartialValueForCulture(editedBlockEditorValues.BlockValue.ContentData, publishedBlockEditorValues.BlockValue.ContentData, culture);
        PublishPartialValueForCulture(editedBlockEditorValues.BlockValue.SettingsData, publishedBlockEditorValues.BlockValue.SettingsData, culture);

        return _jsonSerializer.Serialize(publishedBlockEditorValues.BlockValue);
    }

    private void PublishPartialValueForCulture(List<BlockItemData> editedBlockItems, List<BlockItemData> publishedBlockItems, string? culture)
    {
        // remove all published blocks that are not part of the edited blocks (structure is global)
        publishedBlockItems.RemoveAll(pb => editedBlockItems.Any(eb => eb.Key == pb.Key) is false);

        // merge the edited values into the published values for culture
        foreach (BlockItemData editedBlockItem in editedBlockItems)
        {
            BlockItemData? publishedBlockItem = publishedBlockItems.FirstOrDefault(i => i.Key == editedBlockItem.Key);
            if (publishedBlockItem is null)
            {
                publishedBlockItem = new BlockItemData(
                    editedBlockItem.Key,
                    editedBlockItem.ContentTypeKey,
                    editedBlockItem.ContentTypeAlias);

                // NOTE: this only works because publishedBlockItems is by ref!
                publishedBlockItems.Add(publishedBlockItem);
            }

            foreach (BlockPropertyValue editedBlockPropertyValue in editedBlockItem.Values)
            {
                // is this another editor that supports partial publishing? i.e. blocks within blocks.
                IDataEditor? mergingDataEditor = null;
                var shouldPerformPartialPublish = editedBlockPropertyValue.PropertyType is not null
                                  && _propertyEditors.TryGet(editedBlockPropertyValue.PropertyType.PropertyEditorAlias, out mergingDataEditor)
                                  && mergingDataEditor.ShouldPublishPartialValues(editedBlockPropertyValue.PropertyType);

                if (shouldPerformPartialPublish is false && editedBlockPropertyValue.Culture != culture)
                {
                    // skip for now (irrelevant for the current culture, but might be included in the next pass)
                    continue;
                }

                BlockPropertyValue? publishedBlockPropertyValue = publishedBlockItem
                    .Values
                    .FirstOrDefault(v =>
                        v.Alias == editedBlockPropertyValue.Alias &&
                        v.Culture == editedBlockPropertyValue.Culture &&
                        v.Segment == editedBlockPropertyValue.Segment);

                if (publishedBlockPropertyValue is null)
                {
                    publishedBlockPropertyValue = new BlockPropertyValue
                    {
                        Alias = editedBlockPropertyValue.Alias,
                        Culture = editedBlockPropertyValue.Culture,
                        Segment = editedBlockPropertyValue.Segment
                    };
                    publishedBlockItem.Values.Add(publishedBlockPropertyValue);
                }

                // assign edited value to published value (or perform partial publish, depending on context)
                publishedBlockPropertyValue.Value = shouldPerformPartialPublish is false
                    ? editedBlockPropertyValue.Value
                    : mergingDataEditor!.PublishPartialValueForCulture(editedBlockPropertyValue.Value, publishedBlockPropertyValue.Value, culture);
            }
        }
    }
}
