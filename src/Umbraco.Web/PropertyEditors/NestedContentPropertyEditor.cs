using System;
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
        internal const string ContentTypeAliasPropertyKey = "ncContentTypeAlias";

        public NestedContentPropertyEditor(ILogger logger, Lazy<PropertyEditorCollection> propertyEditors, IDataTypeService dataTypeService)
            : base (logger)
        {
            _propertyEditors = propertyEditors;
            _dataTypeService = dataTypeService;
        }

        // has to be lazy else circular dep in ctor
        private PropertyEditorCollection PropertyEditors => _propertyEditors.Value;

        private static IContentType GetElementType(JObject item)
        {
            var contentTypeAlias = item[ContentTypeAliasPropertyKey]?.ToObject<string>();
            return string.IsNullOrEmpty(contentTypeAlias)
                ? null
                : Current.Services.ContentTypeService.Get(contentTypeAlias);
        }

        #region Pre Value Editor

        protected override IConfigurationEditor CreateConfigurationEditor() => new NestedContentConfigurationEditor();

        #endregion

        #region Value Editor

        protected override IDataValueEditor CreateValueEditor() => new NestedContentPropertyValueEditor(Attribute, PropertyEditors, _dataTypeService);

        internal class NestedContentPropertyValueEditor : DataValueEditor, IDataValueReference
        {
            private readonly PropertyEditorCollection _propertyEditors;
            private readonly IDataTypeService _dataTypeService;

            public NestedContentPropertyValueEditor(DataEditorAttribute attribute, PropertyEditorCollection propertyEditors, IDataTypeService dataTypeService)
                : base(attribute)
            {
                _propertyEditors = propertyEditors;
                _dataTypeService = dataTypeService;
                Validators.Add(new NestedContentValidator(propertyEditors, dataTypeService));
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

            /// <summary>
            /// Method used to iterate over the deserialized property values
            /// </summary>
            /// <param name="propertyValue"></param>
            /// <param name="onIteration"></param>
            internal static List<JObject> IteratePropertyValues(object propertyValue, Action<string, PropertyType, JObject, int> onIteration)
            {
                if (propertyValue == null || string.IsNullOrWhiteSpace(propertyValue.ToString()))
                    return null;

                var value = JsonConvert.DeserializeObject<List<JObject>>(propertyValue.ToString());

                // There was a note here about checking if the result had zero items and if so it would return null, so we'll continue to do that
                // The original note was: "Issue #38 - Keep recursive property lookups working"
                // Which is from the original NC tracker: https://github.com/umco/umbraco-nested-content/issues/38
                // This check should be used everywhere when iterating NC prop values, instead of just the one previous place so that
                // empty values don't get persisted when there is nothing, it should actually be null.
                if (value == null || value.Count == 0)
                    return null;

                var index = 0;

                foreach (var o in value)
                {
                    var propValues = o;

                    // TODO: This is N+1 (although we cache all doc types, it's still not pretty)
                    var contentType = GetElementType(propValues);
                    if (contentType == null)
                        continue;

                    var propertyTypes = contentType.CompositionPropertyTypes.ToDictionary(x => x.Alias, x => x);
                    var propAliases = propValues.Properties().Select(x => x.Name);
                    foreach (var propAlias in propAliases)
                    {
                        propertyTypes.TryGetValue(propAlias, out var propType);
                        onIteration(propAlias, propType, propValues, index);                        
                    }

                    index++;
                }

                return value;
            }

            #region DB to String

            public override string ConvertDbToString(PropertyType propertyType, object propertyValue, IDataTypeService dataTypeService)
            {
                var value = IteratePropertyValues(propertyValue, (string propAlias, PropertyType propType, JObject propValues, int index) =>
                {
                    if (propType == null)
                    {
                        // type not found, and property is not system: just delete the value
                        if (IsSystemPropertyKey(propAlias) == false)
                            propValues[propAlias] = null;
                    }
                    else
                    {
                        try
                        {
                            // convert the value, and store the converted value
                            var propEditor = _propertyEditors[propType.PropertyEditorAlias];
                            var tempConfig = dataTypeService.GetDataType(propType.DataTypeId).Configuration;
                            var valEditor = propEditor.GetValueEditor(tempConfig);
                            var convValue = valEditor.ConvertDbToString(propType, propValues[propAlias]?.ToString(), dataTypeService);
                            propValues[propAlias] = convValue;
                        }
                        catch (InvalidOperationException)
                        {
                            // deal with weird situations by ignoring them (no comment)
                            propValues[propAlias] = null;
                        }
                    }
                });

                if (value == null)
                    return string.Empty;

                return JsonConvert.SerializeObject(value).ToXmlString<string>();
            }

            #endregion

            

            #region Convert database // editor

            // note: there is NO variant support here

            public override object ToEditor(Property property, IDataTypeService dataTypeService, string culture = null, string segment = null)
            {
                var val = property.GetValue(culture, segment);

                var value = IteratePropertyValues(val, (string propAlias, PropertyType propType, JObject propValues, int index) =>
                {
                    if (propType == null)
                    {
                        // type not found, and property is not system: just delete the value
                        if (IsSystemPropertyKey(propAlias) == false)
                            propValues[propAlias] = null;
                    }
                    else
                    {
                        try
                        {
                            // create a temp property with the value
                            // - force it to be culture invariant as NC can't handle culture variant element properties
                            propType.Variations = ContentVariation.Nothing;
                            var tempProp = new Property(propType);
                            tempProp.SetValue(propValues[propAlias] == null ? null : propValues[propAlias].ToString());

                            // convert that temp property, and store the converted value
                            var propEditor = _propertyEditors[propType.PropertyEditorAlias];
                            var tempConfig = dataTypeService.GetDataType(propType.DataTypeId).Configuration;
                            var valEditor = propEditor.GetValueEditor(tempConfig);
                            var convValue = valEditor.ToEditor(tempProp, dataTypeService);
                            propValues[propAlias] = convValue == null ? null : JToken.FromObject(convValue);
                        }
                        catch (InvalidOperationException)
                        {
                            // deal with weird situations by ignoring them (no comment)
                            propValues[propAlias] = null;
                        }
                    }
                });

                if (value == null)
                    return string.Empty;

                // return json
                return value;
            }

            public override object FromEditor(ContentPropertyData editorValue, object currentValue)
            {
                if (editorValue.Value == null || string.IsNullOrWhiteSpace(editorValue.Value.ToString()))
                    return null;

                var value = IteratePropertyValues(editorValue.Value, (string propAlias, PropertyType propType, JObject propValues, int index) =>
                {
                    if (propType == null)
                    {
                        // type not found, and property is not system: just delete the value
                        if (IsSystemPropertyKey(propAlias) == false)
                            propValues[propAlias] = null;
                    }
                    else
                    {
                        // Fetch the property types prevalue
                        var propConfiguration = _dataTypeService.GetDataType(propType.DataTypeId).Configuration;

                        // Lookup the property editor
                        var propEditor = _propertyEditors[propType.PropertyEditorAlias];

                        // Create a fake content property data object
                        var contentPropData = new ContentPropertyData(propValues[propAlias], propConfiguration);

                        // Get the property editor to do it's conversion
                        var newValue = propEditor.GetValueEditor().FromEditor(contentPropData, propValues[propAlias]);

                        // Store the value back
                        propValues[propAlias] = (newValue == null) ? null : JToken.FromObject(newValue);
                    }
                });

                if (value == null)
                    return string.Empty;

                // return json
                return JsonConvert.SerializeObject(value);
            }

            public IEnumerable<UmbracoEntityReference> GetReferences(object value)
            {
                var rawJson = value == null ? string.Empty : value is string str ? str : value.ToString();

                var result = new List<UmbracoEntityReference>();

                var json = IteratePropertyValues(rawJson, (string propAlias, PropertyType propType, JObject propValues, int index) =>
                {
                    if (propType == null) return;

                    var propEditor = _propertyEditors[propType.PropertyEditorAlias];

                    var valueEditor = propEditor?.GetValueEditor();
                    if (!(valueEditor is IDataValueReference reference)) return;

                    var val = propValues[propAlias]?.ToString();

                    var refs = reference.GetReferences(val);

                    result.AddRange(refs);
                });

                return result;
            }

            #endregion
        }

        internal class NestedContentValidator : IValueValidator
        {
            private readonly PropertyEditorCollection _propertyEditors;
            private readonly IDataTypeService _dataTypeService;

            public NestedContentValidator(PropertyEditorCollection propertyEditors, IDataTypeService dataTypeService)
            {
                _propertyEditors = propertyEditors;
                _dataTypeService = dataTypeService;
            }

            public IEnumerable<ValidationResult> Validate(object rawValue, string valueType, object dataTypeConfiguration)
            {
                var validationResults = new List<ValidationResult>();

                NestedContentPropertyValueEditor.IteratePropertyValues(rawValue, (string propKey, PropertyType propType, JObject propValues, int i) =>
                {
                    if (propType == null) return;

                    var config = _dataTypeService.GetDataType(propType.DataTypeId).Configuration;
                    var propertyEditor = _propertyEditors[propType.PropertyEditorAlias];

                    foreach (var validator in propertyEditor.GetValueEditor().Validators)
                    {
                        foreach (var result in validator.Validate(propValues[propKey], propertyEditor.GetValueEditor().ValueType, config))
                        {
                            result.ErrorMessage = "Item " + (i + 1) + " '" + propType.Name + "' " + result.ErrorMessage;
                            validationResults.Add(result);
                        }
                    }

                    // Check mandatory
                    if (propType.Mandatory)
                    {
                        if (propValues[propKey] == null)
                            validationResults.Add(new ValidationResult("Item " + (i + 1) + " '" + propType.Name + "' cannot be null", new[] { propKey }));
                        else if (propValues[propKey].ToString().IsNullOrWhiteSpace() || (propValues[propKey].Type == JTokenType.Array && !propValues[propKey].HasValues))
                            validationResults.Add(new ValidationResult("Item " + (i + 1) + " '" + propType.Name + "' cannot be empty", new[] { propKey }));
                    }

                    // Check regex
                    if (!propType.ValidationRegExp.IsNullOrWhiteSpace()
                        && propValues[propKey] != null && !propValues[propKey].ToString().IsNullOrWhiteSpace())
                    {
                        var regex = new Regex(propType.ValidationRegExp);
                        if (!regex.IsMatch(propValues[propKey].ToString()))
                        {
                            validationResults.Add(new ValidationResult("Item " + (i + 1) + " '" + propType.Name + "' is invalid, it does not match the correct pattern", new[] { propKey }));
                        }
                    }
                });

                return validationResults;
            }
        }

        #endregion

        private static bool IsSystemPropertyKey(string propKey)
        {
            return propKey == "name" || propKey == "key" || propKey == ContentTypeAliasPropertyKey;
        }
    }
}
