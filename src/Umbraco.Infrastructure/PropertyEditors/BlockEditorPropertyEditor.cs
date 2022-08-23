// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Abstract class for block editor based editors
/// </summary>
public abstract class BlockEditorPropertyEditor : DataEditor
{
    public const string ContentTypeKeyPropertyKey = "contentTypeKey";
    public const string UdiPropertyKey = "udi";

    public BlockEditorPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        PropertyEditorCollection propertyEditors)
        : base(dataValueEditorFactory)
    {
        PropertyEditors = propertyEditors;
        SupportsReadOnly = true;
    }

    private PropertyEditorCollection PropertyEditors { get; }

    #region Value Editor

    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<BlockEditorPropertyValueEditor>(Attribute!);

    internal class BlockEditorPropertyValueEditor : DataValueEditor, IDataValueReference
    {
        private readonly BlockEditorValues _blockEditorValues;
        private readonly IDataTypeService _dataTypeService;
        private readonly ILogger<BlockEditorPropertyValueEditor> _logger;
        private readonly PropertyEditorCollection _propertyEditors;

        public BlockEditorPropertyValueEditor(
            DataEditorAttribute attribute,
            PropertyEditorCollection propertyEditors,
            IDataTypeService dataTypeService,
            IContentTypeService contentTypeService,
            ILocalizedTextService textService,
            ILogger<BlockEditorPropertyValueEditor> logger,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            IPropertyValidationService propertyValidationService)
            : base(textService, shortStringHelper, jsonSerializer, ioHelper, attribute)
        {
            _propertyEditors = propertyEditors;
            _dataTypeService = dataTypeService;
            _logger = logger;

            _blockEditorValues = new BlockEditorValues(new BlockListEditorDataConverter(), contentTypeService, _logger);
            Validators.Add(new BlockEditorValidator(propertyValidationService, _blockEditorValues, contentTypeService));
            Validators.Add(new MinMaxValidator(_blockEditorValues, textService));
        }

        public IEnumerable<UmbracoEntityReference> GetReferences(object? value)
        {
            var rawJson = value == null ? string.Empty : value is string str ? str : value.ToString();

            var result = new List<UmbracoEntityReference>();
            BlockEditorData? blockEditorData = _blockEditorValues.DeserializeAndClean(rawJson);
            if (blockEditorData == null)
            {
                return Enumerable.Empty<UmbracoEntityReference>();
            }

            // loop through all content and settings data
            foreach (BlockItemData row in blockEditorData.BlockValue.ContentData.Concat(blockEditorData.BlockValue
                         .SettingsData))
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
            var valEditors = new Dictionary<int, IDataValueEditor>();

            BlockEditorData? blockEditorData;
            try
            {
                blockEditorData = _blockEditorValues.DeserializeAndClean(val);
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

            void MapBlockItemData(List<BlockItemData> items)
            {
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
                                "ToEditor removed property value {PropertyKey} in row {RowId} for property type {PropertyTypeAlias}", prop.Key, row.Key, property.PropertyType.Alias);
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

            MapBlockItemData(blockEditorData.BlockValue.ContentData);
            MapBlockItemData(blockEditorData.BlockValue.SettingsData);

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
                blockEditorData = _blockEditorValues.DeserializeAndClean(editorValue.Value);
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

            void MapBlockItemData(List<BlockItemData> items)
            {
                foreach (BlockItemData row in items)
                {
                    foreach (KeyValuePair<string, BlockItemData.BlockPropertyValue> prop in row.PropertyValues)
                    {
                        // Fetch the property types prevalue
                        var propConfiguration = _dataTypeService.GetDataType(prop.Value.PropertyType.DataTypeId)
                            ?.Configuration;

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

            MapBlockItemData(blockEditorData.BlockValue.ContentData);
            MapBlockItemData(blockEditorData.BlockValue.SettingsData);

            // return json
            return JsonConvert.SerializeObject(blockEditorData.BlockValue, Formatting.None);
        }

        #endregion
    }

    internal class BlockEditorValidator : ComplexEditorValidator
    {
        private readonly BlockEditorValues _blockEditorValues;
        private readonly IContentTypeService _contentTypeService;

        public BlockEditorValidator(
            IPropertyValidationService propertyValidationService,
            BlockEditorValues blockEditorValues,
            IContentTypeService contentTypeService)
            : base(propertyValidationService)
        {
            _blockEditorValues = blockEditorValues;
            _contentTypeService = contentTypeService;
        }

        protected override IEnumerable<ElementTypeValidationModel> GetElementTypeValidation(object? value)
        {
            BlockEditorData? blockEditorData = _blockEditorValues.DeserializeAndClean(value);
            if (blockEditorData != null)
            {
                // There is no guarantee that the client will post data for every property defined in the Element Type but we still
                // need to validate that data for each property especially for things like 'required' data to work.
                // Lookup all element types for all content/settings and then we can populate any empty properties.
                var allElements = blockEditorData.BlockValue.ContentData.Concat(blockEditorData.BlockValue.SettingsData)
                    .ToList();
                var allElementTypes = _contentTypeService.GetAll(allElements.Select(x => x.ContentTypeKey).ToArray())
                    .ToDictionary(x => x.Key);

                foreach (BlockItemData row in allElements)
                {
                    if (!allElementTypes.TryGetValue(row.ContentTypeKey, out IContentType? elementType))
                    {
                        throw new InvalidOperationException($"No element type found with key {row.ContentTypeKey}");
                    }

                    // now ensure missing properties
                    foreach (IPropertyType elementTypeProp in elementType.CompositionPropertyTypes)
                    {
                        if (!row.PropertyValues.ContainsKey(elementTypeProp.Alias))
                        {
                            // set values to null
                            row.PropertyValues[elementTypeProp.Alias] =
                                new BlockItemData.BlockPropertyValue(null, elementTypeProp);
                            row.RawPropertyValues[elementTypeProp.Alias] = null;
                        }
                    }

                    var elementValidation = new ElementTypeValidationModel(row.ContentTypeAlias, row.Key);
                    foreach (KeyValuePair<string, BlockItemData.BlockPropertyValue> prop in row.PropertyValues)
                    {
                        elementValidation.AddPropertyTypeValidation(
                            new PropertyTypeValidationModel(prop.Value.PropertyType, prop.Value.Value));
                    }

                    yield return elementValidation;
                }
            }
        }
    }

    /// <summary>
    ///     Validates the min/max of the block editor
    /// </summary>
    private class MinMaxValidator : IValueValidator
    {
        private readonly BlockEditorValues _blockEditorValues;
        private readonly ILocalizedTextService _textService;

        public MinMaxValidator(BlockEditorValues blockEditorValues, ILocalizedTextService textService)
        {
            _blockEditorValues = blockEditorValues;
            _textService = textService;
        }

        public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration)
        {
            var blockConfig = (BlockListConfiguration?)dataTypeConfiguration;
            if (blockConfig == null)
            {
                yield break;
            }

            BlockListConfiguration.NumberRange? validationLimit = blockConfig.ValidationLimit;
            if (validationLimit == null)
            {
                yield break;
            }

            BlockEditorData? blockEditorData = _blockEditorValues.DeserializeAndClean(value);

            if ((blockEditorData == null && validationLimit.Min.HasValue && validationLimit.Min > 0)
                || (blockEditorData != null && validationLimit.Min.HasValue &&
                    blockEditorData.Layout?.Count() < validationLimit.Min))
            {
                yield return new ValidationResult(
                    _textService.Localize(
                        "validation",
                        "entriesShort",
                        new[]
                        {
                            validationLimit.Min.ToString(),
                            (validationLimit.Min - (blockEditorData?.Layout?.Count() ?? 0)).ToString(),
                        }),
                    new[] { "minCount" });
            }

            if (blockEditorData != null && validationLimit.Max.HasValue &&
                blockEditorData.Layout?.Count() > validationLimit.Max)
            {
                yield return new ValidationResult(
                    _textService.Localize(
                        "validation",
                        "entriesExceed",
                        new[]
                        {
                            validationLimit.Max.ToString(),
                            (blockEditorData.Layout.Count() - validationLimit.Max).ToString(),
                        }),
                    new[] { "maxCount" });
            }
        }
    }

    /// <summary>
    ///     Used to deserialize json values and clean up any values based on the existence of element types and layout
    ///     structure
    /// </summary>
    internal class BlockEditorValues
    {
        private readonly Lazy<Dictionary<Guid, IContentType>> _contentTypes;
        private readonly BlockEditorDataConverter _dataConverter;
        private readonly ILogger _logger;

        public BlockEditorValues(BlockEditorDataConverter dataConverter, IContentTypeService contentTypeService, ILogger logger)
        {
            _contentTypes =
                new Lazy<Dictionary<Guid, IContentType>>(() => contentTypeService.GetAll().ToDictionary(c => c.Key));
            _dataConverter = dataConverter;
            _logger = logger;
        }

        public BlockEditorData? DeserializeAndClean(object? propertyValue)
        {
            if (propertyValue == null || string.IsNullOrWhiteSpace(propertyValue.ToString()))
            {
                return null;
            }

            BlockEditorData blockEditorData = _dataConverter.Deserialize(propertyValue.ToString()!);

            if (blockEditorData.BlockValue.ContentData.Count == 0)
            {
                // if there's no content ensure there's no settings too
                blockEditorData.BlockValue.SettingsData.Clear();
                return null;
            }

            var contentTypePropertyTypes = new Dictionary<string, Dictionary<string, IPropertyType>>();

            // filter out any content that isn't referenced in the layout references
            foreach (BlockItemData block in blockEditorData.BlockValue.ContentData.Where(x =>
                         blockEditorData.References.Any(r => x.Udi is not null && r.ContentUdi == x.Udi)))
            {
                ResolveBlockItemData(block, contentTypePropertyTypes);
            }

            // filter out any settings that isn't referenced in the layout references
            foreach (BlockItemData block in blockEditorData.BlockValue.SettingsData.Where(x =>
                         blockEditorData.References.Any(r =>
                             r.SettingsUdi is not null && x.Udi is not null && r.SettingsUdi == x.Udi)))
            {
                ResolveBlockItemData(block, contentTypePropertyTypes);
            }

            // remove blocks that couldn't be resolved
            blockEditorData.BlockValue.ContentData.RemoveAll(x => x.ContentTypeAlias.IsNullOrWhiteSpace());
            blockEditorData.BlockValue.SettingsData.RemoveAll(x => x.ContentTypeAlias.IsNullOrWhiteSpace());

            return blockEditorData;
        }

        private IContentType? GetElementType(BlockItemData item)
        {
            _contentTypes.Value.TryGetValue(item.ContentTypeKey, out IContentType? contentType);
            return contentType;
        }

        private bool ResolveBlockItemData(
            BlockItemData block,
            Dictionary<string, Dictionary<string, IPropertyType>> contentTypePropertyTypes)
        {
            IContentType? contentType = GetElementType(block);
            if (contentType == null)
            {
                return false;
            }

            // get the prop types for this content type but keep a dictionary of found ones so we don't have to keep re-looking and re-creating
            // objects on each iteration.
            if (!contentTypePropertyTypes.TryGetValue(
                contentType.Alias,
                out Dictionary<string, IPropertyType>? propertyTypes))
            {
                propertyTypes = contentTypePropertyTypes[contentType.Alias] =
                    contentType.CompositionPropertyTypes.ToDictionary(x => x.Alias, x => x);
            }

            var propValues = new Dictionary<string, BlockItemData.BlockPropertyValue>();

            // find any keys that are not real property types and remove them
            foreach (KeyValuePair<string, object?> prop in block.RawPropertyValues.ToList())
            {
                // doesn't exist so remove it
                if (!propertyTypes.TryGetValue(prop.Key, out IPropertyType? propType))
                {
                    block.RawPropertyValues.Remove(prop.Key);
                    _logger.LogWarning(
                        "The property {PropertyKey} for block {BlockKey} was removed because the property type {PropertyTypeAlias} was not found on {ContentTypeAlias}",
                        prop.Key,
                        block.Key,
                        prop.Key,
                        contentType.Alias);
                }
                else
                {
                    // set the value to include the resolved property type
                    propValues[prop.Key] = new BlockItemData.BlockPropertyValue(prop.Value, propType);
                }
            }

            block.ContentTypeAlias = contentType.Alias;
            block.PropertyValues = propValues;

            return true;
        }
    }

    #endregion
}
