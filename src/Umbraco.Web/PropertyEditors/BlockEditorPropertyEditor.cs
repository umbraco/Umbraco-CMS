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

                foreach (var row in _blockEditorValues.GetPropertyValues(rawJson))
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
                foreach (var row in _blockEditorValues.GetPropertyValues(value))
                {
                    var elementValidation = new ElementTypeValidationModel(row.ContentTypeAlias, row.Id);
                    foreach (var prop in row.PropertyValues)
                    {
                        elementValidation.AddPropertyTypeValidation(
                            new PropertyTypeValidationModel(prop.Value.PropertyType, prop.Value.Value));
                    }
                    yield return elementValidation;
                }
            }
        }

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

            public IReadOnlyList<BlockValue> GetPropertyValues(object propertyValue)
            {
                if (propertyValue == null || string.IsNullOrWhiteSpace(propertyValue.ToString()))
                    return new List<BlockValue>();

                var converted = _dataConverter.Convert(propertyValue.ToString());

                if (converted.Blocks.Count == 0)
                    return new List<BlockValue>();

                var contentTypePropertyTypes = new Dictionary<string, Dictionary<string, PropertyType>>();
                var result = new List<BlockValue>();

                foreach(var block in converted.Blocks)
                {
                    var contentType = GetElementType(block);
                    if (contentType == null)
                        continue;

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

                    result.Add(new BlockValue
                    {
                        ContentTypeAlias = contentType.Alias,
                        PropertyValues = propValues,
                        Id = ((GuidUdi)block.Udi).Guid
                    });
                }

                return result;
            }

            /// <summary>
            /// Used during deserialization to populate the property value/property type of a nested content row property
            /// </summary>
            internal class BlockPropertyValue
            {
                public object Value { get; set; }
                public PropertyType PropertyType { get; set; }
            }

            /// <summary>
            /// Used during deserialization to populate the content type alias and property values of a block
            /// </summary>
            internal class BlockValue
            {
                public Guid Id { get; set; }
                public string ContentTypeAlias { get; set; }
                public IDictionary<string, BlockPropertyValue> PropertyValues { get; set; } = new Dictionary<string, BlockPropertyValue>();
            }

        }

        #endregion

    }
}
