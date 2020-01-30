using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

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
        internal const string ContentTypeAliasPropertyKey = "ncContentTypeAlias";

        public NestedContentPropertyEditor(ILogger logger, Lazy<PropertyEditorCollection> propertyEditors, IDataTypeService dataTypeService, IContentTypeService contentTypeService)
            : base (logger)
        {
            _propertyEditors = propertyEditors;
            _dataTypeService = dataTypeService;
            _contentTypeService = contentTypeService;
        }

        // has to be lazy else circular dep in ctor
        private PropertyEditorCollection PropertyEditors => _propertyEditors.Value;

        #region Pre Value Editor

        protected override IConfigurationEditor CreateConfigurationEditor() => new NestedContentConfigurationEditor();

        #endregion

        #region Value Editor

        protected override IDataValueEditor CreateValueEditor() => new NestedContentPropertyValueEditor(Attribute, PropertyEditors, _dataTypeService, _contentTypeService);

        internal class NestedContentPropertyValueEditor : DataValueEditor, IDataValueReference
        {
            private readonly PropertyEditorCollection _propertyEditors;
            private readonly IDataTypeService _dataTypeService;
            private readonly NestedContentValues _nestedContentValues;
            
            public NestedContentPropertyValueEditor(DataEditorAttribute attribute, PropertyEditorCollection propertyEditors, IDataTypeService dataTypeService, IContentTypeService contentTypeService)
                : base(attribute)
            {
                _propertyEditors = propertyEditors;
                _dataTypeService = dataTypeService;
                _nestedContentValues = new NestedContentValues(contentTypeService);
                Validators.Add(new NestedContentValidator(propertyEditors, dataTypeService, _nestedContentValues));
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
                var vals = _nestedContentValues.GetPropertyValues(propertyValue, out var deserialized).ToList();

                if (vals.Count == 0)
                    return string.Empty;

                foreach (var row in vals)
                {
                    if (row.PropType == null)
                    {
                        // type not found, and property is not system: just delete the value
                        if (IsSystemPropertyKey(row.PropKey) == false)
                            row.JsonRowValue[row.PropKey] = null;
                    }
                    else
                    {
                        try
                        {
                            // convert the value, and store the converted value
                            var propEditor = _propertyEditors[row.PropType.PropertyEditorAlias];
                            if (propEditor == null) continue;

                            var tempConfig = dataTypeService.GetDataType(row.PropType.DataTypeId).Configuration;
                            var valEditor = propEditor.GetValueEditor(tempConfig);
                            var convValue = valEditor.ConvertDbToString(row.PropType, row.JsonRowValue[row.PropKey]?.ToString(), dataTypeService);
                            row.JsonRowValue[row.PropKey] = convValue;
                        }
                        catch (InvalidOperationException)
                        {
                            // deal with weird situations by ignoring them (no comment)
                            row.JsonRowValue[row.PropKey] = null;
                        }
                    }
                }

                return JsonConvert.SerializeObject(deserialized).ToXmlString<string>();
            }

            #endregion

            

            #region Convert database // editor

            // note: there is NO variant support here

            public override object ToEditor(Property property, IDataTypeService dataTypeService, string culture = null, string segment = null)
            {
                var val = property.GetValue(culture, segment);

                var vals = _nestedContentValues.GetPropertyValues(val, out var deserialized).ToList();

                if (vals.Count == 0)
                    return string.Empty;

                foreach (var row in vals)
                {
                    if (row.PropType == null)
                    {
                        // type not found, and property is not system: just delete the value
                        if (IsSystemPropertyKey(row.PropKey) == false)
                            row.JsonRowValue[row.PropKey] = null;
                    }
                    else
                    {
                        try
                        {
                            // create a temp property with the value
                            // - force it to be culture invariant as NC can't handle culture variant element properties
                            row.PropType.Variations = ContentVariation.Nothing;
                            var tempProp = new Property(row.PropType);
                            tempProp.SetValue(row.JsonRowValue[row.PropKey] == null ? null : row.JsonRowValue[row.PropKey].ToString());

                            // convert that temp property, and store the converted value
                            var propEditor = _propertyEditors[row.PropType.PropertyEditorAlias];
                            if (propEditor == null)
                            {
                                row.JsonRowValue[row.PropKey] = tempProp.GetValue()?.ToString();
                                continue;
                            }

                            var tempConfig = dataTypeService.GetDataType(row.PropType.DataTypeId).Configuration;
                            var valEditor = propEditor.GetValueEditor(tempConfig);
                            var convValue = valEditor.ToEditor(tempProp, dataTypeService);
                            row.JsonRowValue[row.PropKey] = convValue == null ? null : JToken.FromObject(convValue);
                        }
                        catch (InvalidOperationException)
                        {
                            // deal with weird situations by ignoring them (no comment)
                            row.JsonRowValue[row.PropKey] = null;
                        }
                    }
                }

                // return json
                return deserialized;
            }

            public override object FromEditor(ContentPropertyData editorValue, object currentValue)
            {
                if (editorValue.Value == null || string.IsNullOrWhiteSpace(editorValue.Value.ToString()))
                    return null;

                var vals = _nestedContentValues.GetPropertyValues(editorValue.Value, out var deserialized).ToList();

                if (vals.Count == 0)
                    return string.Empty;

                foreach (var row in vals)
                {
                    if (row.PropType == null)
                    {
                        // type not found, and property is not system: just delete the value
                        if (IsSystemPropertyKey(row.PropKey) == false)
                            row.JsonRowValue[row.PropKey] = null;
                    }
                    else
                    {
                        // Fetch the property types prevalue
                        var propConfiguration = _dataTypeService.GetDataType(row.PropType.DataTypeId).Configuration;

                        // Lookup the property editor
                        var propEditor = _propertyEditors[row.PropType.PropertyEditorAlias];
                        if (propEditor == null) continue;

                        // Create a fake content property data object
                        var contentPropData = new ContentPropertyData(row.JsonRowValue[row.PropKey], propConfiguration);

                        // Get the property editor to do it's conversion
                        var newValue = propEditor.GetValueEditor().FromEditor(contentPropData, row.JsonRowValue[row.PropKey]);

                        // Store the value back
                        row.JsonRowValue[row.PropKey] = (newValue == null) ? null : JToken.FromObject(newValue);
                    }
                }

                // return json
                return JsonConvert.SerializeObject(deserialized);
            }
            #endregion

            public IEnumerable<UmbracoEntityReference> GetReferences(object value)
            {
                var rawJson = value == null ? string.Empty : value is string str ? str : value.ToString();

                var result = new List<UmbracoEntityReference>();

                foreach (var row in _nestedContentValues.GetPropertyValues(rawJson, out _))
                {
                    if (row.PropType == null) continue;

                    var propEditor = _propertyEditors[row.PropType.PropertyEditorAlias];

                    var valueEditor = propEditor?.GetValueEditor();
                    if (!(valueEditor is IDataValueReference reference)) continue;

                    var val = row.JsonRowValue[row.PropKey]?.ToString();

                    var refs = reference.GetReferences(val);

                    result.AddRange(refs);
                }

                return result;
            }
        }

        internal class NestedContentValidator : IValueValidator
        {
            private readonly PropertyEditorCollection _propertyEditors;
            private readonly IDataTypeService _dataTypeService;
            private readonly NestedContentValues _nestedContentValues;

            public NestedContentValidator(PropertyEditorCollection propertyEditors, IDataTypeService dataTypeService, NestedContentValues nestedContentValues)
            {
                _propertyEditors = propertyEditors;
                _dataTypeService = dataTypeService;
                _nestedContentValues = nestedContentValues;
            }

            public IEnumerable<ValidationResult> Validate(object rawValue, string valueType, object dataTypeConfiguration)
            {
                var validationResults = new List<ValidationResult>();

                foreach(var row in _nestedContentValues.GetPropertyValues(rawValue, out _))
                {
                    if (row.PropType == null) continue;

                    var config = _dataTypeService.GetDataType(row.PropType.DataTypeId).Configuration;
                    var propertyEditor = _propertyEditors[row.PropType.PropertyEditorAlias];
                    if (propertyEditor == null) continue;

                    foreach (var validator in propertyEditor.GetValueEditor().Validators)
                    {
                        foreach (var result in validator.Validate(row.JsonRowValue[row.PropKey], propertyEditor.GetValueEditor().ValueType, config))
                        {
                            result.ErrorMessage = "Item " + (row.RowIndex + 1) + " '" + row.PropType.Name + "' " + result.ErrorMessage;
                            validationResults.Add(result);
                        }
                    }

                    // Check mandatory
                    if (row.PropType.Mandatory)
                    {
                        if (row.JsonRowValue[row.PropKey] == null)
                        {
                            var message = string.IsNullOrWhiteSpace(row.PropType.MandatoryMessage)
                                                      ? $"'{row.PropType.Name}' cannot be null"
                                                      : row.PropType.MandatoryMessage;
                            validationResults.Add(new ValidationResult($"Item {(row.RowIndex + 1)}: {message}", new[] { row.PropKey }));
                        }                            
                        else if (row.JsonRowValue[row.PropKey].ToString().IsNullOrWhiteSpace() || (row.JsonRowValue[row.PropKey].Type == JTokenType.Array && !row.JsonRowValue[row.PropKey].HasValues))
                        {
                            var message = string.IsNullOrWhiteSpace(row.PropType.MandatoryMessage)
                                                      ? $"'{row.PropType.Name}' cannot be empty"
                                                      : row.PropType.MandatoryMessage;
                            validationResults.Add(new ValidationResult($"Item {(row.RowIndex + 1)}: {message}", new[] { row.PropKey }));
                        }   
                    }

                    // Check regex
                    if (!row.PropType.ValidationRegExp.IsNullOrWhiteSpace()
                        && row.JsonRowValue[row.PropKey] != null && !row.JsonRowValue[row.PropKey].ToString().IsNullOrWhiteSpace())
                    {
                        var regex = new Regex(row.PropType.ValidationRegExp);
                        if (!regex.IsMatch(row.JsonRowValue[row.PropKey].ToString()))
                        {
                            var message = string.IsNullOrWhiteSpace(row.PropType.ValidationRegExpMessage)
                                                      ? $"'{row.PropType.Name}' is invalid, it does not match the correct pattern"
                                                      : row.PropType.ValidationRegExpMessage;
                           validationResults.Add(new ValidationResult($"Item {(row.RowIndex + 1)}: {message}", new[] { row.PropKey }));
                        }
                    }
                }

                return validationResults;
            }
        }

        internal class NestedContentValues
        {
            private readonly Lazy<Dictionary<string, IContentType>> _contentTypes;

            public NestedContentValues(IContentTypeService contentTypeService)
            {
                _contentTypes = new Lazy<Dictionary<string, IContentType>>(() => contentTypeService.GetAll().ToDictionary(c => c.Alias));
            }

            private IContentType GetElementType(JObject item)
            {
                var contentTypeAlias = item[ContentTypeAliasPropertyKey]?.ToObject<string>() ?? string.Empty;
                _contentTypes.Value.TryGetValue(contentTypeAlias, out var contentType);
                return contentType;
            }

            public IEnumerable<RowValue> GetPropertyValues(object propertyValue, out List<JObject> deserialized)
            {
                var rowValues = new List<RowValue>();

                deserialized = null;

                if (propertyValue == null || string.IsNullOrWhiteSpace(propertyValue.ToString()))
                    return Enumerable.Empty<RowValue>();

                deserialized = JsonConvert.DeserializeObject<List<JObject>>(propertyValue.ToString());

                // There was a note here about checking if the result had zero items and if so it would return null, so we'll continue to do that
                // The original note was: "Issue #38 - Keep recursive property lookups working"
                // Which is from the original NC tracker: https://github.com/umco/umbraco-nested-content/issues/38
                // This check should be used everywhere when iterating NC prop values, instead of just the one previous place so that
                // empty values don't get persisted when there is nothing, it should actually be null.
                if (deserialized == null || deserialized.Count == 0)
                    return Enumerable.Empty<RowValue>();

                var index = 0;

                foreach (var o in deserialized)
                {
                    var propValues = o;

                    var contentType = GetElementType(propValues);
                    if (contentType == null)
                        continue;

                    var propertyTypes = contentType.CompositionPropertyTypes.ToDictionary(x => x.Alias, x => x);
                    var propAliases = propValues.Properties().Select(x => x.Name);
                    foreach (var propAlias in propAliases)
                    {
                        propertyTypes.TryGetValue(propAlias, out var propType);
                        rowValues.Add(new RowValue(propAlias, propType, propValues, index));
                    }
                    index++;
                }

                return rowValues;
            }

            internal class RowValue
            {
                public RowValue(string propKey, PropertyType propType, JObject propValues, int index)
                {
                    PropKey = propKey ?? throw new ArgumentNullException(nameof(propKey));
                    PropType = propType;
                    JsonRowValue = propValues ?? throw new ArgumentNullException(nameof(propValues));
                    RowIndex = index;
                }

                /// <summary>
                /// The current property key being iterated for the row value
                /// </summary>
                public string PropKey { get; }

                /// <summary>
                /// The <see cref="PropertyType"/> of the value (if any), this may be null
                /// </summary>
                public PropertyType PropType { get; }

                /// <summary>
                /// The json values for the current row
                /// </summary>
                public JObject JsonRowValue { get; }

                /// <summary>
                /// The Nested Content row index
                /// </summary>
                public int RowIndex { get; }
            }
        }

        #endregion

        private static bool IsSystemPropertyKey(string propKey)
        {
            return propKey == "name" || propKey == "key" || propKey == ContentTypeAliasPropertyKey;
        }
    }
}
