using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Blocks;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using static Umbraco.Core.Models.Blocks.BlockEditorData;
using static Umbraco.Core.Models.Blocks.BlockItemData;

namespace Umbraco.Web.PropertyEditors
{

    /// <summary>
    /// Abstract class for block editor based editors
    /// </summary>
    public abstract class BlockEditorPropertyEditor : DataEditor
    {
        public const string ContentTypeKeyPropertyKey = "contentTypeKey";
        public const string UdiPropertyKey = "udi";
        private readonly ILocalizedTextService _localizedTextService;
        private readonly Lazy<PropertyEditorCollection> _propertyEditors;
        private readonly IDataTypeService _dataTypeService;
        private readonly IContentTypeService _contentTypeService;

        public BlockEditorPropertyEditor(ILogger logger, Lazy<PropertyEditorCollection> propertyEditors, IDataTypeService dataTypeService, IContentTypeService contentTypeService, ILocalizedTextService localizedTextService)
            : base(logger)
        {
            _localizedTextService = localizedTextService;
            _propertyEditors = propertyEditors;
            _dataTypeService = dataTypeService;
            _contentTypeService = contentTypeService;
        }

        // has to be lazy else circular dep in ctor
        private PropertyEditorCollection PropertyEditors => _propertyEditors.Value;

        #region Value Editor

        protected override IDataValueEditor CreateValueEditor() => new BlockEditorPropertyValueEditor(Attribute, PropertyEditors, _dataTypeService, _contentTypeService, _localizedTextService);

        internal class BlockEditorPropertyValueEditor : DataValueEditor, IDataValueReference
        {
            private readonly PropertyEditorCollection _propertyEditors;
            private readonly IDataTypeService _dataTypeService; // TODO: Not used yet but we'll need it to fill in the FromEditor/ToEditor
            private readonly BlockEditorValues _blockEditorValues;

            public BlockEditorPropertyValueEditor(DataEditorAttribute attribute, PropertyEditorCollection propertyEditors, IDataTypeService dataTypeService, IContentTypeService contentTypeService, ILocalizedTextService textService)
                : base(attribute)
            {
                _propertyEditors = propertyEditors;
                _dataTypeService = dataTypeService;
                _blockEditorValues = new BlockEditorValues(new BlockListEditorDataConverter(), contentTypeService);
                Validators.Add(new BlockEditorValidator(_blockEditorValues, propertyEditors, dataTypeService, textService));
            }

            public IEnumerable<UmbracoEntityReference> GetReferences(object value)
            {
                var rawJson = value == null ? string.Empty : value is string str ? str : value.ToString();

                var result = new List<UmbracoEntityReference>();
                var blockEditorData = _blockEditorValues.DeserializeAndClean(rawJson);
                if (blockEditorData == null)
                    return Enumerable.Empty<UmbracoEntityReference>();

                // TODO: What about Settings?
                foreach (var row in blockEditorData.BlockValue.ContentData)
                {
                    foreach (var prop in row.PropertyValues)
                    {
                        var propEditor = _propertyEditors[prop.Value.PropertyType.PropertyEditorAlias];

                        var valueEditor = propEditor?.GetValueEditor();
                        if (!(valueEditor is IDataValueReference reference)) continue;

                        var val = prop.Value.Value?.ToString();

                        var refs = reference.GetReferences(val);

                        result.AddRange(refs);
                    }
                }

                return result;
            }

            #region Convert database // editor

            // note: there is NO variant support here

            /// <summary>
            /// Ensure that sub-editor values are translated through their ToEditor methods
            /// </summary>
            /// <param name="property"></param>
            /// <param name="dataTypeService"></param>
            /// <param name="culture"></param>
            /// <param name="segment"></param>
            /// <returns></returns>
            public override object ToEditor(Property property, IDataTypeService dataTypeService, string culture = null, string segment = null)
            {
                var val = property.GetValue(culture, segment);

                BlockEditorData blockEditorData;
                try
                {
                    blockEditorData = _blockEditorValues.DeserializeAndClean(val);
                }
                catch (JsonSerializationException)
                {
                    // if this occurs it means the data is invalid, shouldn't happen but has happened if we change the data format.
                    return string.Empty;
                }

                if (blockEditorData == null || blockEditorData.BlockValue.ContentData.Count == 0)
                    return string.Empty;

                foreach (var row in blockEditorData.BlockValue.ContentData)
                {
                    foreach (var prop in row.PropertyValues)
                    {
                        try
                        {
                            // create a temp property with the value
                            // - force it to be culture invariant as the block editor can't handle culture variant element properties
                            prop.Value.PropertyType.Variations = ContentVariation.Nothing;
                            var tempProp = new Property(prop.Value.PropertyType);

                            tempProp.SetValue(prop.Value.Value);

                            // convert that temp property, and store the converted value
                            var propEditor = _propertyEditors[prop.Value.PropertyType.PropertyEditorAlias];
                            if (propEditor == null)
                            {
                                // NOTE: This logic was borrowed from Nested Content and I'm unsure why it exists.
                                // if the property editor doesn't exist I think everything will break anyways?
                                // update the raw value since this is what will get serialized out
                                row.RawPropertyValues[prop.Key] = tempProp.GetValue()?.ToString();
                                continue;
                            }

                            var tempConfig = dataTypeService.GetDataType(prop.Value.PropertyType.DataTypeId).Configuration;
                            var valEditor = propEditor.GetValueEditor(tempConfig);
                            var convValue = valEditor.ToEditor(tempProp, dataTypeService);

                            // update the raw value since this is what will get serialized out
                            row.RawPropertyValues[prop.Key] = convValue;
                        }
                        catch (InvalidOperationException)
                        {
                            // deal with weird situations by ignoring them (no comment)
                            row.PropertyValues.Remove(prop.Key);
                        }
                    }
                }

                // return json convertable object
                return blockEditorData.BlockValue;
            }

            /// <summary>
            /// Ensure that sub-editor values are translated through their FromEditor methods
            /// </summary>
            /// <param name="editorValue"></param>
            /// <param name="currentValue"></param>
            /// <returns></returns>
            public override object FromEditor(ContentPropertyData editorValue, object currentValue)
            {
                if (editorValue.Value == null || string.IsNullOrWhiteSpace(editorValue.Value.ToString()))
                    return null;

                BlockEditorData blockEditorData;
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
                    return string.Empty;

                foreach (var row in blockEditorData.BlockValue.ContentData)
                {
                    foreach (var prop in row.PropertyValues)
                    {
                        // Fetch the property types prevalue
                        var propConfiguration = _dataTypeService.GetDataType(prop.Value.PropertyType.DataTypeId).Configuration;

                        // Lookup the property editor
                        var propEditor = _propertyEditors[prop.Value.PropertyType.PropertyEditorAlias];
                        if (propEditor == null) continue;

                        // Create a fake content property data object
                        var contentPropData = new ContentPropertyData(prop.Value.Value, propConfiguration);

                        // Get the property editor to do it's conversion
                        var newValue = propEditor.GetValueEditor().FromEditor(contentPropData, prop.Value.Value);

                        // update the raw value since this is what will get serialized out
                        row.RawPropertyValues[prop.Key] = newValue;
                    }
                }

                // return json
                return JsonConvert.SerializeObject(blockEditorData.BlockValue);
            }

            #endregion
        }

        internal class BlockEditorValidator : ComplexEditorValidator
        {
            private readonly BlockEditorValues _blockEditorValues;

            public BlockEditorValidator(BlockEditorValues blockEditorValues, PropertyEditorCollection propertyEditors, IDataTypeService dataTypeService, ILocalizedTextService textService) : base(propertyEditors, dataTypeService, textService)
            {
                _blockEditorValues = blockEditorValues;
            }

            protected override IEnumerable<ElementTypeValidationModel> GetElementTypeValidation(object value)
            {
                var blockEditorData = _blockEditorValues.DeserializeAndClean(value);
                if (blockEditorData != null)
                {
                    foreach (var row in blockEditorData.BlockValue.ContentData)
                    {
                        var elementValidation = new ElementTypeValidationModel(row.ContentTypeAlias, row.Key);
                        foreach (var prop in row.PropertyValues)
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
        /// Used to deserialize json values and clean up any values based on the existence of element types and layout structure
        /// </summary>
        internal class BlockEditorValues
        {
            private readonly Lazy<Dictionary<Guid, IContentType>> _contentTypes;
            private readonly BlockEditorDataConverter _dataConverter;

            public BlockEditorValues(BlockEditorDataConverter dataConverter, IContentTypeService contentTypeService)
            {
                _contentTypes = new Lazy<Dictionary<Guid, IContentType>>(() => contentTypeService.GetAll().ToDictionary(c => c.Key));
                _dataConverter = dataConverter;
            }

            private IContentType GetElementType(BlockItemData item)
            {
                _contentTypes.Value.TryGetValue(item.ContentTypeKey, out var contentType);
                return contentType;
            }

            public BlockEditorData DeserializeAndClean(object propertyValue)
            {
                if (propertyValue == null || string.IsNullOrWhiteSpace(propertyValue.ToString()))
                    return null;

                var blockEditorData = _dataConverter.Deserialize(propertyValue.ToString());

                if (blockEditorData.BlockValue.ContentData.Count == 0)
                {
                    // if there's no content ensure there's no settings too
                    blockEditorData.BlockValue.SettingsData.Clear();
                    return null;
                }

                var contentTypePropertyTypes = new Dictionary<string, Dictionary<string, PropertyType>>();

                // filter out any content that isn't referenced in the layout references
                foreach(var block in blockEditorData.BlockValue.ContentData.Where(x => blockEditorData.References.Any(r => r.ContentUdi == x.Udi)))
                {
                    ResolveBlockItemData(block, contentTypePropertyTypes);
                }
                // filter out any settings that isn't referenced in the layout references
                foreach (var block in blockEditorData.BlockValue.SettingsData.Where(x => blockEditorData.References.Any(r => r.SettingsUdi == x.Udi)))
                {
                    ResolveBlockItemData(block, contentTypePropertyTypes);
                }

                // remove blocks that couldn't be resolved
                blockEditorData.BlockValue.ContentData.RemoveAll(x => x.ContentTypeAlias.IsNullOrWhiteSpace());
                blockEditorData.BlockValue.SettingsData.RemoveAll(x => x.ContentTypeAlias.IsNullOrWhiteSpace());

                return blockEditorData;
            }

            private bool ResolveBlockItemData(BlockItemData block, Dictionary<string, Dictionary<string, PropertyType>> contentTypePropertyTypes)
            {
                var contentType = GetElementType(block);
                if (contentType == null)
                    return false;

                // get the prop types for this content type but keep a dictionary of found ones so we don't have to keep re-looking and re-creating
                // objects on each iteration.
                if (!contentTypePropertyTypes.TryGetValue(contentType.Alias, out var propertyTypes))
                    propertyTypes = contentTypePropertyTypes[contentType.Alias] = contentType.CompositionPropertyTypes.ToDictionary(x => x.Alias, x => x);

                var propValues = new Dictionary<string, BlockPropertyValue>();

                // find any keys that are not real property types and remove them
                foreach (var prop in block.RawPropertyValues.ToList())
                {
                    // doesn't exist so remove it
                    if (!propertyTypes.TryGetValue(prop.Key, out var propType))
                    {
                        block.RawPropertyValues.Remove(prop.Key);
                    }
                    else
                    {
                        // set the value to include the resolved property type
                        propValues[prop.Key] = new BlockPropertyValue
                        {
                            PropertyType = propType,
                            Value = prop.Value
                        };
                    }
                }

                block.ContentTypeAlias = contentType.Alias;
                block.PropertyValues = propValues;

                return true;
            }
        }

        #endregion

    }
}
