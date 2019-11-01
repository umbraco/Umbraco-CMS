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
                var vals = _nestedContentValues.GetPropertyValues(propertyValue).ToList();

                if (vals.Count == 0)
                    return string.Empty;

                foreach (var row in vals)
                {
                    if (row.PropType == null)
                    {
                        // type not found, and property is not system: just delete the value
                        if (IsSystemPropertyKey(row.PropKey) == false)
                            row.PropValues[row.PropKey] = null;
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
                            var convValue = valEditor.ConvertDbToString(row.PropType, row.PropValues[row.PropKey]?.ToString(), dataTypeService);
                            row.PropValues[row.PropKey] = convValue;
                        }
                        catch (InvalidOperationException)
                        {
                            // deal with weird situations by ignoring them (no comment)
                            row.PropValues[row.PropKey] = null;
                        }
                    }
                }

                return JsonConvert.SerializeObject(vals).ToXmlString<string>();
            }

            #endregion

            

            #region Convert database // editor

            // note: there is NO variant support here

            public override object ToEditor(Property property, IDataTypeService dataTypeService, string culture = null, string segment = null)
            {
                var val = property.GetValue(culture, segment);

                var vals = _nestedContentValues.GetPropertyValues(val).ToList();

                if (vals.Count == 0)
                    return string.Empty;

                foreach (var row in vals)
                {
                    if (row.PropType == null)
                    {
                        // type not found, and property is not system: just delete the value
                        if (IsSystemPropertyKey(row.PropKey) == false)
                            row.PropValues[row.PropKey] = null;
                    }
                    else
                    {
                        try
                        {
                            // create a temp property with the value
                            // - force it to be culture invariant as NC can't handle culture variant element properties
                            row.PropType.Variations = ContentVariation.Nothing;
                            var tempProp = new Property(row.PropType);
                            tempProp.SetValue(row.PropValues[row.PropKey] == null ? null : row.PropValues[row.PropKey].ToString());

                            // convert that temp property, and store the converted value
                            var propEditor = _propertyEditors[row.PropType.PropertyEditorAlias];
                            if (propEditor == null)
                            {
                                row.PropValues[row.PropKey] = tempProp.GetValue()?.ToString();
                                continue;
                            }

                            var tempConfig = dataTypeService.GetDataType(row.PropType.DataTypeId).Configuration;
                            var valEditor = propEditor.GetValueEditor(tempConfig);
                            var convValue = valEditor.ToEditor(tempProp, dataTypeService);
                            row.PropValues[row.PropKey] = convValue == null ? null : JToken.FromObject(convValue);
                        }
                        catch (InvalidOperationException)
                        {
                            // deal with weird situations by ignoring them (no comment)
                            row.PropValues[row.PropKey] = null;
                        }
                    }
                }

                // return json
                return vals;
            }

            public override object FromEditor(ContentPropertyData editorValue, object currentValue)
            {
                if (editorValue.Value == null || string.IsNullOrWhiteSpace(editorValue.Value.ToString()))
                    return null;

                var vals = _nestedContentValues.GetPropertyValues(editorValue.Value).ToList();

                if (vals.Count == 0)
                    return string.Empty;

                foreach (var row in vals)
                {
                    if (row.PropType == null)
                    {
                        // type not found, and property is not system: just delete the value
                        if (IsSystemPropertyKey(row.PropKey) == false)
                            row.PropValues[row.PropKey] = null;
                    }
                    else
                    {
                        // Fetch the property types prevalue
                        var propConfiguration = _dataTypeService.GetDataType(row.PropType.DataTypeId).Configuration;

                        // Lookup the property editor
                        var propEditor = _propertyEditors[row.PropType.PropertyEditorAlias];
                        if (propEditor == null) continue;

                        // Create a fake content property data object
                        var contentPropData = new ContentPropertyData(row.PropValues[row.PropKey], propConfiguration);

                        // Get the property editor to do it's conversion
                        var newValue = propEditor.GetValueEditor().FromEditor(contentPropData, row.PropValues[row.PropKey]);

                        // Store the value back
                        row.PropValues[row.PropKey] = (newValue == null) ? null : JToken.FromObject(newValue);
                    }
                }

                // return json
                return JsonConvert.SerializeObject(vals);
            }

            public IEnumerable<UmbracoEntityReference> GetReferences(object value)
            {
                var rawJson = value == null ? string.Empty : value is string str ? str : value.ToString();

                var result = new List<UmbracoEntityReference>();

                foreach (var row in _nestedContentValues.GetPropertyValues(rawJson))
                {
                    if (row.PropType == null) continue;

                    var propEditor = _propertyEditors[row.PropType.PropertyEditorAlias];

                    var valueEditor = propEditor?.GetValueEditor();
                    if (!(valueEditor is IDataValueReference reference)) continue;

                    var val = row.PropValues[row.PropKey]?.ToString();

                    var refs = reference.GetReferences(val);

                    result.AddRange(refs);
                }

                return result;
            }

            #endregion
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

                foreach(var row in _nestedContentValues.GetPropertyValues(rawValue))
                {
                    if (row.PropType == null) continue;

                    var config = _dataTypeService.GetDataType(row.PropType.DataTypeId).Configuration;
                    var propertyEditor = _propertyEditors[row.PropType.PropertyEditorAlias];
                    if (propertyEditor == null) continue;

                    foreach (var validator in propertyEditor.GetValueEditor().Validators)
                    {
                        foreach (var result in validator.Validate(row.PropValues[row.PropKey], propertyEditor.GetValueEditor().ValueType, config))
                        {
                            result.ErrorMessage = "Item " + (row.Index + 1) + " '" + row.PropType.Name + "' " + result.ErrorMessage;
                            validationResults.Add(result);
                        }
                    }

                    // Check mandatory
                    if (row.PropType.Mandatory)
                    {
                        if (row.PropValues[row.PropKey] == null)
                            validationResults.Add(new ValidationResult("Item " + (row.Index + 1) + " '" + row.PropType.Name + "' cannot be null", new[] { row.PropKey }));
                        else if (row.PropValues[row.PropKey].ToString().IsNullOrWhiteSpace() || (row.PropValues[row.PropKey].Type == JTokenType.Array && !row.PropValues[row.PropKey].HasValues))
                            validationResults.Add(new ValidationResult("Item " + (row.Index + 1) + " '" + row.PropType.Name + "' cannot be empty", new[] { row.PropKey }));
                    }

                    // Check regex
                    if (!row.PropType.ValidationRegExp.IsNullOrWhiteSpace()
                        && row.PropValues[row.PropKey] != null && !row.PropValues[row.PropKey].ToString().IsNullOrWhiteSpace())
                    {
                        var regex = new Regex(row.PropType.ValidationRegExp);
                        if (!regex.IsMatch(row.PropValues[row.PropKey].ToString()))
                        {
                            validationResults.Add(new ValidationResult("Item " + (row.Index + 1) + " '" + row.PropType.Name + "' is invalid, it does not match the correct pattern", new[] { row.PropKey }));
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

            public IEnumerable<RowValue> GetPropertyValues(object propertyValue)
            {
                if (propertyValue == null || string.IsNullOrWhiteSpace(propertyValue.ToString()))
                    yield break;

                var value = JsonConvert.DeserializeObject<List<JObject>>(propertyValue.ToString());

                // There was a note here about checking if the result had zero items and if so it would return null, so we'll continue to do that
                // The original note was: "Issue #38 - Keep recursive property lookups working"
                // Which is from the original NC tracker: https://github.com/umco/umbraco-nested-content/issues/38
                // This check should be used everywhere when iterating NC prop values, instead of just the one previous place so that
                // empty values don't get persisted when there is nothing, it should actually be null.
                if (value == null || value.Count == 0)
                    yield break;

                var index = 0;

                foreach (var o in value)
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
                        yield return new RowValue(propAlias, propType, propValues, index);
                    }
                    index++;
                }
            }

            internal class RowValue
            {
                public RowValue(string propKey, PropertyType propType, JObject propValues, int index)
                {
                    PropKey = propKey ?? throw new ArgumentNullException(nameof(propKey));
                    PropType = propType ?? throw new ArgumentNullException(nameof(propType));
                    PropValues = propValues ?? throw new ArgumentNullException(nameof(propValues));
                    Index = index;
                }

                public string PropKey { get; }
                public PropertyType PropType { get; }
                public JObject PropValues { get; }
                public int Index { get; }
            }
        }

        #endregion

        private static bool IsSystemPropertyKey(string propKey)
        {
            return propKey == "name" || propKey == "key" || propKey == ContentTypeAliasPropertyKey;
        }
    }
}
