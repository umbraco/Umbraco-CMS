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

public abstract class BlockValuePropertyValueEditorBase<TValue, TLayout> : DataValueEditor, IDataValueReference, IDataValueTags
    where TValue : BlockValue<TLayout>, new()
    where TLayout : class, IBlockLayoutItem, new()
{
    private readonly IDataTypeConfigurationCache _dataTypeConfigurationCache;
    private readonly PropertyEditorCollection _propertyEditors;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly DataValueReferenceFactoryCollection _dataValueReferenceFactoryCollection;
    private readonly BlockEditorVarianceHandler _blockEditorVarianceHandler;
    private BlockEditorValues<TValue, TLayout>? _blockEditorValues;
    private readonly ILanguageService _languageService;

    [Obsolete("Please use the non-obsolete constructor. Will be removed in V16.")]
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
        : this(
            propertyEditors,
            dataTypeConfigurationCache,
            shortStringHelper,
            jsonSerializer,
            dataValueReferenceFactoryCollection,
            StaticServiceProvider.Instance.GetRequiredService<BlockEditorVarianceHandler>(),
            StaticServiceProvider.Instance.GetRequiredService<ILanguageService>(),
            ioHelper,
            attribute
            )
    {
    }

    protected BlockValuePropertyValueEditorBase(
        PropertyEditorCollection propertyEditors,
        IDataTypeConfigurationCache dataTypeConfigurationCache,
        IShortStringHelper shortStringHelper,
        IJsonSerializer jsonSerializer,
        DataValueReferenceFactoryCollection dataValueReferenceFactoryCollection,
        BlockEditorVarianceHandler blockEditorVarianceHandler,
        ILanguageService languageService,
        IIOHelper ioHelper,
        DataEditorAttribute attribute)
        : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
    {
        _propertyEditors = propertyEditors;
        _dataTypeConfigurationCache = dataTypeConfigurationCache;
        _jsonSerializer = jsonSerializer;
        _dataValueReferenceFactoryCollection = dataValueReferenceFactoryCollection;
        _blockEditorVarianceHandler = blockEditorVarianceHandler;
        _languageService = languageService;
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

                var tagLanguageId = blockPropertyValue.Culture is not null
                    ? _languageService.GetAsync(blockPropertyValue.Culture).GetAwaiter().GetResult()?.Id
                    : languageId;
                result.AddRange(tagsProvider.GetTags(blockPropertyValue.Value, configuration, tagLanguageId));
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
        _blockEditorVarianceHandler.AlignExposeVariance(blockValue);
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
                _blockEditorVarianceHandler.AlignPropertyVarianceAsync(blockPropertyValue, propertyType, culture).GetAwaiter().GetResult();

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

    internal virtual object? MergePartialPropertyValueForCulture(object? sourceValue, object? targetValue, string? culture)
    {
        if (sourceValue is null)
        {
            return null;
        }

        // parse the source value as block editor data
        BlockEditorData<TValue, TLayout>? sourceBlockEditorValues = BlockEditorValues.DeserializeAndClean(sourceValue);
        if (sourceBlockEditorValues?.Layout is null)
        {
            return null;
        }

        // parse the target value as block editor data (fallback to an empty set of block editor data)
        BlockEditorData<TValue, TLayout> targetBlockEditorValues =
            (targetValue is not null ? BlockEditorValues.DeserializeAndClean(targetValue) : null)
            ?? new BlockEditorData<TValue, TLayout>([], CreateWithLayout(sourceBlockEditorValues.Layout));

        TValue mergeResult = MergeBlockEditorDataForCulture(sourceBlockEditorValues.BlockValue, targetBlockEditorValues.BlockValue, culture);
        return _jsonSerializer.Serialize(mergeResult);
    }

    protected TValue MergeBlockEditorDataForCulture(TValue sourceBlockValue, TValue targetBlockValue, string? culture)
    {
        // structure is global, layout and expose follows structure
        targetBlockValue.Layout = sourceBlockValue.Layout;
        targetBlockValue.Expose = sourceBlockValue.Expose;

        MergePartialPropertyValueForCulture(sourceBlockValue.ContentData, targetBlockValue.ContentData, culture);
        MergePartialPropertyValueForCulture(sourceBlockValue.SettingsData, targetBlockValue.SettingsData, culture);

        return targetBlockValue;
    }

    private void MergePartialPropertyValueForCulture(List<BlockItemData> sourceBlockItems, List<BlockItemData> targetBlockItems, string? culture)
    {
        // remove all target blocks that are not part of the source blocks (structure is global)
        targetBlockItems.RemoveAll(pb => sourceBlockItems.Any(eb => eb.Key == pb.Key) is false);

        // merge the source values into the target values for culture
        foreach (BlockItemData sourceBlockItem in sourceBlockItems)
        {
            BlockItemData? targetBlockItem = targetBlockItems.FirstOrDefault(i => i.Key == sourceBlockItem.Key);
            if (targetBlockItem is null)
            {
                targetBlockItem = new BlockItemData(
                    sourceBlockItem.Key,
                    sourceBlockItem.ContentTypeKey,
                    sourceBlockItem.ContentTypeAlias);

                // NOTE: this only works because targetBlockItem is by ref!
                targetBlockItems.Add(targetBlockItem);
            }

            foreach (BlockPropertyValue sourceBlockPropertyValue in sourceBlockItem.Values)
            {
                // is this another editor that supports partial merging? i.e. blocks within blocks.
                IDataEditor? mergingDataEditor = null;
                var shouldPerformPartialMerge = sourceBlockPropertyValue.PropertyType is not null
                                  && _propertyEditors.TryGet(sourceBlockPropertyValue.PropertyType.PropertyEditorAlias, out mergingDataEditor)
                                  && mergingDataEditor.CanMergePartialPropertyValues(sourceBlockPropertyValue.PropertyType);

                if (shouldPerformPartialMerge is false && sourceBlockPropertyValue.Culture != culture)
                {
                    // skip for now (irrelevant for the current culture, but might be included in the next pass)
                    continue;
                }

                BlockPropertyValue? targetBlockPropertyValue = targetBlockItem
                    .Values
                    .FirstOrDefault(v =>
                        v.Alias == sourceBlockPropertyValue.Alias &&
                        v.Culture == sourceBlockPropertyValue.Culture &&
                        v.Segment == sourceBlockPropertyValue.Segment);

                if (targetBlockPropertyValue is null)
                {
                    targetBlockPropertyValue = new BlockPropertyValue
                    {
                        Alias = sourceBlockPropertyValue.Alias,
                        Culture = sourceBlockPropertyValue.Culture,
                        Segment = sourceBlockPropertyValue.Segment
                    };
                    targetBlockItem.Values.Add(targetBlockPropertyValue);
                }

                // assign source value to target value (or perform partial merge, depending on context)
                targetBlockPropertyValue.Value = shouldPerformPartialMerge is false
                    ? sourceBlockPropertyValue.Value
                    : mergingDataEditor!.MergePartialPropertyValueForCulture(sourceBlockPropertyValue.Value, targetBlockPropertyValue.Value, culture);
            }
        }
    }
}
