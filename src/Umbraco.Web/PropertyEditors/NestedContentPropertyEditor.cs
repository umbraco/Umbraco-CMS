using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.PropertyEditors.Validation;

namespace Umbraco.Web.PropertyEditors
{

    /// <summary>
    /// Represents a nested content property editor.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.NestedContent,
        "Nested Content",
        "nestedcontent", 
        ValueType = ValueTypes.Json,
        Group = Constants.PropertyEditors.Groups.Lists,
        Icon = "icon-thumbnail-list")]
    public class NestedContentPropertyEditor : DataEditor
    {
        private readonly Lazy<PropertyEditorCollection> _propertyEditors;
        private readonly IDataTypeService _dataTypeService;
        private readonly IContentTypeService _contentTypeService;
        private readonly ILocalizedTextService _localizedTextService;
        internal const string ContentTypeAliasPropertyKey = "ncContentTypeAlias";

        [Obsolete("Use the constructor specifying all parameters instead")]
        public NestedContentPropertyEditor(ILogger logger, Lazy<PropertyEditorCollection> propertyEditors, IDataTypeService dataTypeService, IContentTypeService contentTypeService)
            : this(logger, propertyEditors, dataTypeService, contentTypeService, Current.Services.TextService) { }

        public NestedContentPropertyEditor(ILogger logger, Lazy<PropertyEditorCollection> propertyEditors, IDataTypeService dataTypeService, IContentTypeService contentTypeService, ILocalizedTextService localizedTextService)
            : base (logger)
        {
            _propertyEditors = propertyEditors;
            _dataTypeService = dataTypeService;
            _contentTypeService = contentTypeService;
            _localizedTextService = localizedTextService;
        }

        // has to be lazy else circular dep in ctor
        private PropertyEditorCollection PropertyEditors => _propertyEditors.Value;

        #region Pre Value Editor

        protected override IConfigurationEditor CreateConfigurationEditor() => new NestedContentConfigurationEditor();

        #endregion

        #region Value Editor

        protected override IDataValueEditor CreateValueEditor() => new NestedContentPropertyValueEditor(Attribute, PropertyEditors, _dataTypeService, _contentTypeService, _localizedTextService, Logger);

        internal class NestedContentPropertyValueEditor : DataValueEditor, IDataValueReference
        {
            private readonly PropertyEditorCollection _propertyEditors;
            private readonly IDataTypeService _dataTypeService;
            private readonly ILogger _logger;
            private readonly NestedContentValues _nestedContentValues;
            
            public NestedContentPropertyValueEditor(DataEditorAttribute attribute, PropertyEditorCollection propertyEditors, IDataTypeService dataTypeService, IContentTypeService contentTypeService, ILocalizedTextService textService, ILogger logger)
                : base(attribute)
            {
                _propertyEditors = propertyEditors;
                _dataTypeService = dataTypeService;
                _logger = logger;
                _nestedContentValues = new NestedContentValues(contentTypeService);
                Validators.Add(new NestedContentValidator(_nestedContentValues, propertyEditors, dataTypeService, textService, contentTypeService));
            }

            /// <inheritdoc />
            public override object Configuration
            {
                get => base.Configuration;
                set
                {
                    if (value == null)
                        throw new ArgumentNullException(nameof(value));
                    if (!(value is NestedContentConfiguration configuration))
                        throw new ArgumentException($"Expected a {typeof(NestedContentConfiguration).Name} instance, but got {value.GetType().Name}.", nameof(value));
                    base.Configuration = value;

                    HideLabel = configuration.HideLabel.TryConvertTo<bool>().Result;
                }
            }

            #region DB to String

            public override string ConvertDbToString(PropertyType propertyType, object propertyValue, IDataTypeService dataTypeService)
            {
                var rows = _nestedContentValues.GetPropertyValues(propertyValue);

                if (rows.Count == 0)
                    return null;

                foreach (var row in rows.ToList())
                {
                    foreach(var prop in row.PropertyValues.ToList())
                    {
                        try
                        {
                            // convert the value, and store the converted value
                            var propEditor = _propertyEditors[prop.Value.PropertyType.PropertyEditorAlias];
                            if (propEditor == null) continue;

                            var tempConfig = dataTypeService.GetDataType(prop.Value.PropertyType.DataTypeId).Configuration;
                            var valEditor = propEditor.GetValueEditor(tempConfig);
                            var convValue = valEditor.ConvertDbToString(prop.Value.PropertyType, prop.Value.Value, dataTypeService);

                            // update the raw value since this is what will get serialized out
                            row.RawPropertyValues[prop.Key] = convValue;
                        }
                        catch (InvalidOperationException ex)
                        {
                            // deal with weird situations by ignoring them (no comment)
                            row.RawPropertyValues.Remove(prop.Key);
                            _logger.Warn<NestedContentPropertyValueEditor, string, Guid, string>(
                                ex,
                                "ConvertDbToString removed property value {PropertyKey} in row {RowId} for property type {PropertyTypeAlias}",
                                prop.Key, row.Id, propertyType.Alias);
                        }
                    }
                }

                return JsonConvert.SerializeObject(rows, Formatting.None).ToXmlString<string>();
            }

            #endregion



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
                var valEditors = new Dictionary<int, IDataValueEditor>();

                var rows = _nestedContentValues.GetPropertyValues(val);

                if (rows.Count == 0)
                    return string.Empty;

                foreach (var row in rows.ToList())
                {
                    foreach(var prop in row.PropertyValues.ToList())
                    {
                        try
                        {
                            // create a temp property with the value
                            // - force it to be culture invariant as NC can't handle culture variant element properties
                            prop.Value.PropertyType.Variations = ContentVariation.Nothing;
                            var tempProp = new Property(prop.Value.PropertyType);

                            tempProp.SetValue(prop.Value.Value);

                            // convert that temp property, and store the converted value
                            var propEditor = _propertyEditors[prop.Value.PropertyType.PropertyEditorAlias];
                            if (propEditor == null)
                            {
                                // update the raw value since this is what will get serialized out
                                row.RawPropertyValues[prop.Key] = tempProp.GetValue()?.ToString();
                                continue;
                            }

                            var dataTypeId = prop.Value.PropertyType.DataTypeId;
                            if (!valEditors.TryGetValue(dataTypeId, out var valEditor))
                            {
                                var tempConfig = dataTypeService.GetDataType(dataTypeId).Configuration;
                                valEditor = propEditor.GetValueEditor(tempConfig);

                                valEditors.Add(dataTypeId, valEditor);
                            }

                            var convValue = valEditor.ToEditor(tempProp, dataTypeService);

                            // update the raw value since this is what will get serialized out
                            row.RawPropertyValues[prop.Key] = convValue == null ? null : JToken.FromObject(convValue);
                        }
                        catch (InvalidOperationException ex)
                        {
                            // deal with weird situations by ignoring them (no comment)
                            row.RawPropertyValues.Remove(prop.Key);
                            _logger.Warn<NestedContentPropertyValueEditor, string, Guid, string>(
                                ex,
                                "ToEditor removed property value {PropertyKey} in row {RowId} for property type {PropertyTypeAlias}",
                                prop.Key, row.Id, property.PropertyType.Alias);
                        }
                    }
                }

                // return the object, there's a native json converter for this so it will serialize correctly
                return rows;
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

                var rows = _nestedContentValues.GetPropertyValues(editorValue.Value);

                if (rows.Count == 0)
                    return null;

                foreach (var row in rows.ToList())
                {
                    foreach(var prop in row.PropertyValues.ToList())
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
                        row.RawPropertyValues[prop.Key] = (newValue == null) ? null : JToken.FromObject(newValue);
                    }
                }

                // return json
                return JsonConvert.SerializeObject(rows, Formatting.None);
            }

            #endregion

            public IEnumerable<UmbracoEntityReference> GetReferences(object value)
            {
                var rawJson = value == null ? string.Empty : value is string str ? str : value.ToString();

                var result = new List<UmbracoEntityReference>();

                foreach (var row in _nestedContentValues.GetPropertyValues(rawJson))
                {
                    foreach(var prop in row.PropertyValues)
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

        /// <summary>
        /// Validator for nested content to ensure that all nesting of editors is validated
        /// </summary>
        internal class NestedContentValidator : ComplexEditorValidator
        {
            private readonly NestedContentValues _nestedContentValues;
            private readonly IContentTypeService _contentTypeService;

            public NestedContentValidator(NestedContentValues nestedContentValues, PropertyEditorCollection propertyEditors, IDataTypeService dataTypeService, ILocalizedTextService textService, IContentTypeService contentTypeService)
                : base(propertyEditors, dataTypeService, textService)
            {
                _nestedContentValues = nestedContentValues;
                _contentTypeService = contentTypeService;
            }

            protected override IEnumerable<ElementTypeValidationModel> GetElementTypeValidation(object value)
            {
                var rows = _nestedContentValues.GetPropertyValues(value);
                if (rows.Count == 0) yield break;

                // There is no guarantee that the client will post data for every property defined in the Element Type but we still
                // need to validate that data for each property especially for things like 'required' data to work.
                // Lookup all element types for all content/settings and then we can populate any empty properties.
                var allElementAliases = rows.Select(x => x.ContentTypeAlias).ToList();
                // unfortunately we need to get all content types and post filter - but they are cached so its ok, there's
                // no overload to lookup by many aliases.
                var allElementTypes = _contentTypeService.GetAll().Where(x => allElementAliases.Contains(x.Alias)).ToDictionary(x => x.Alias);

                foreach (var row in rows)
                {
                    if (!allElementTypes.TryGetValue(row.ContentTypeAlias, out var elementType))
                        throw new InvalidOperationException($"No element type found with alias {row.ContentTypeAlias}");

                    // now ensure missing properties
                    foreach (var elementTypeProp in elementType.CompositionPropertyTypes)
                    {
                        if (!row.PropertyValues.ContainsKey(elementTypeProp.Alias))
                        {
                            // set values to null
                            row.PropertyValues[elementTypeProp.Alias] = new NestedContentValues.NestedContentPropertyValue
                            {
                                PropertyType = elementTypeProp,
                                Value = null
                            };
                            row.RawPropertyValues[elementTypeProp.Alias] = null;
                        }
                    }

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

        /// <summary>
        /// Used to deserialize the nested content serialized value
        /// </summary>
        internal class NestedContentValues
        {
            private readonly Lazy<Dictionary<string, IContentType>> _contentTypes;

            public NestedContentValues(IContentTypeService contentTypeService)
            {
                _contentTypes = new Lazy<Dictionary<string, IContentType>>(() => contentTypeService.GetAll().ToDictionary(c => c.Alias));
            }

            private IContentType GetElementType(NestedContentRowValue item)
            {
                _contentTypes.Value.TryGetValue(item.ContentTypeAlias, out var contentType);
                return contentType;
            }

            /// <summary>
            /// Deserialize the raw json property value
            /// </summary>
            /// <param name="propertyValue"></param>
            /// <returns></returns>
            public IReadOnlyList<NestedContentRowValue> GetPropertyValues(object propertyValue)
            {
                if (propertyValue == null || string.IsNullOrWhiteSpace(propertyValue.ToString()))
                    return new List<NestedContentRowValue>();
                
                 if (!propertyValue.ToString().DetectIsJson())
                    return new List<NestedContentRowValue>();
                    
                var rowValues = JsonConvert.DeserializeObject<List<NestedContentRowValue>>(propertyValue.ToString());

                // There was a note here about checking if the result had zero items and if so it would return null, so we'll continue to do that
                // The original note was: "Issue #38 - Keep recursive property lookups working"
                // Which is from the original NC tracker: https://github.com/umco/umbraco-nested-content/issues/38
                // This check should be used everywhere when iterating NC prop values, instead of just the one previous place so that
                // empty values don't get persisted when there is nothing, it should actually be null.
                if (rowValues == null || rowValues.Count == 0)
                    return new List<NestedContentRowValue>();

                var contentTypePropertyTypes = new Dictionary<string, Dictionary<string, PropertyType>>();

                foreach (var row in rowValues)
                {
                    var contentType = GetElementType(row);
                    if (contentType == null)
                        continue;

                    // get the prop types for this content type but keep a dictionary of found ones so we don't have to keep re-looking and re-creating
                    // objects on each iteration.
                    if (!contentTypePropertyTypes.TryGetValue(contentType.Alias, out var propertyTypes))
                        propertyTypes = contentTypePropertyTypes[contentType.Alias] = contentType.CompositionPropertyTypes.ToDictionary(x => x.Alias, x => x);

                    // find any keys that are not real property types and remove them
                    if (row.RawPropertyValues != null)
                    {
                        foreach (var prop in row.RawPropertyValues.ToList())
                        {
                            if (IsSystemPropertyKey(prop.Key)) continue;

                            // doesn't exist so remove it
                            if (!propertyTypes.TryGetValue(prop.Key, out var propType))
                            {
                                row.RawPropertyValues.Remove(prop.Key);
                            }
                            else
                            {
                                // set the value to include the resolved property type
                                row.PropertyValues[prop.Key] = new NestedContentPropertyValue
                                {
                                    PropertyType = propType,
                                    Value = prop.Value
                                };
                            }
                        }
                    }
                }

                return rowValues;
            }

            /// <summary>
            /// Used during deserialization to populate the property value/property type of a nested content row property
            /// </summary>
            internal class NestedContentPropertyValue
            {
                public object Value { get; set; }
                public PropertyType PropertyType { get; set; }
            }

            /// <summary>
            /// Used to deserialize a nested content row
            /// </summary>
            internal class NestedContentRowValue
            {
                [JsonProperty("key")]
                public Guid Id{ get; set; }

                [JsonProperty("name")]
                public string Name { get; set; }

                [JsonProperty("ncContentTypeAlias")]
                public string ContentTypeAlias { get; set; }

                /// <summary>
                /// The remaining properties will be serialized to a dictionary
                /// </summary>
                /// <remarks>
                /// The JsonExtensionDataAttribute is used to put the non-typed properties into a bucket
                /// http://www.newtonsoft.com/json/help/html/DeserializeExtensionData.htm
                /// NestedContent serializes to string, int, whatever eg
                ///   "stringValue":"Some String","numericValue":125,"otherNumeric":null
                /// </remarks>
                [JsonExtensionData]
                public IDictionary<string, object> RawPropertyValues { get; set; }

                /// <summary>
                /// Used during deserialization to convert the raw property data into data with a property type context
                /// </summary>
                [JsonIgnore]
                public IDictionary<string, NestedContentPropertyValue> PropertyValues { get; set; } = new Dictionary<string, NestedContentPropertyValue>();
            }
        }

        #endregion

        private static bool IsSystemPropertyKey(string propKey)
        {
            return propKey == "name" || propKey == "key" || propKey == ContentTypeAliasPropertyKey;
        }
    }
}
