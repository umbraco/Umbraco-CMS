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
    [DataEditor(Constants.PropertyEditors.Aliases.NestedContent, "Nested Content", "nestedcontent", ValueType = "JSON", Group = "lists", Icon = "icon-thumbnail-list")]
    public class NestedContentPropertyEditor : DataEditor
    {
        private readonly Lazy<PropertyEditorCollection> _propertyEditors;

        internal const string ContentTypeAliasPropertyKey = "ncContentTypeAlias";

        public NestedContentPropertyEditor(ILogger logger, Lazy<PropertyEditorCollection> propertyEditors)
            : base (logger)
        {
            _propertyEditors = propertyEditors;
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

        protected override IDataValueEditor CreateValueEditor() => new NestedContentPropertyValueEditor(Attribute, PropertyEditors);

        internal class NestedContentPropertyValueEditor : DataValueEditor
        {
            private readonly PropertyEditorCollection _propertyEditors;

            public NestedContentPropertyValueEditor(DataEditorAttribute attribute, PropertyEditorCollection propertyEditors)
                : base(attribute)
            {
                _propertyEditors = propertyEditors;
                Validators.Add(new NestedContentValidator(propertyEditors));
            }

            internal ServiceContext Services => Current.Services;

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
                if (propertyValue == null || string.IsNullOrWhiteSpace(propertyValue.ToString()))
                    return string.Empty;

                var value = JsonConvert.DeserializeObject<List<object>>(propertyValue.ToString());
                if (value == null)
                    return string.Empty;

                foreach (var o in value)
                {
                    var propValues = (JObject) o;

                    var contentType = GetElementType(propValues);
                    if (contentType == null)
                        continue;

                    var propAliases = propValues.Properties().Select(x => x.Name).ToArray();
                    foreach (var propAlias in propAliases)
                    {
                        var propType = contentType.CompositionPropertyTypes.FirstOrDefault(x => x.Alias == propAlias);
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
                    }
                }

                return JsonConvert.SerializeObject(value).ToXmlString<string>();
            }

            #endregion

            #region Convert database // editor

            // note: there is NO variant support here

            public override object ToEditor(Property property, IDataTypeService dataTypeService, string culture = null, string segment = null)
            {
                var val = property.GetValue(culture, segment);
                if (val == null || string.IsNullOrWhiteSpace(val.ToString()))
                    return string.Empty;

                var value = JsonConvert.DeserializeObject<List<object>>(val.ToString());
                if (value == null)
                    return string.Empty;

                foreach (var o in value)
                {
                    var propValues = (JObject) o;

                    var contentType = GetElementType(propValues);
                    if (contentType == null)
                        continue;

                    var propAliases = propValues.Properties().Select(x => x.Name).ToArray();
                    foreach (var propAlias in propAliases)
                    {
                        var propType = contentType.CompositionPropertyTypes.FirstOrDefault(x => x.Alias == propAlias);
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

                    }
                }

                // return json
                return value;
            }

            public override object FromEditor(ContentPropertyData editorValue, object currentValue)
            {
                if (editorValue.Value == null || string.IsNullOrWhiteSpace(editorValue.Value.ToString()))
                    return null;

                var value = JsonConvert.DeserializeObject<List<object>>(editorValue.Value.ToString());
                if (value == null)
                    return null;

                // Issue #38 - Keep recursive property lookups working
                if (!value.Any())
                    return null;

                // Process value
                for (var i = 0; i < value.Count; i++)
                {
                    var o = value[i];
                    var propValues = ((JObject)o);

                    var contentType = GetElementType(propValues);
                    if (contentType == null)
                    {
                        continue;
                    }

                    var propValueKeys = propValues.Properties().Select(x => x.Name).ToArray();

                    foreach (var propKey in propValueKeys)
                    {
                        var propType = contentType.CompositionPropertyTypes.FirstOrDefault(x => x.Alias == propKey);
                        if (propType == null)
                        {
                            if (IsSystemPropertyKey(propKey) == false)
                            {
                                // Property missing so just delete the value
                                propValues[propKey] = null;
                            }
                        }
                        else
                        {
                            // Fetch the property types prevalue
                            var propConfiguration = Services.DataTypeService.GetDataType(propType.DataTypeId).Configuration;

                            // Lookup the property editor
                            var propEditor = _propertyEditors[propType.PropertyEditorAlias];

                            // Create a fake content property data object
                            var contentPropData = new ContentPropertyData(propValues[propKey], propConfiguration);

                            // Get the property editor to do it's conversion
                            var newValue = propEditor.GetValueEditor().FromEditor(contentPropData, propValues[propKey]);

                            // Store the value back
                            propValues[propKey] = (newValue == null) ? null : JToken.FromObject(newValue);
                        }

                    }
                }

                return JsonConvert.SerializeObject(value);
            }

            #endregion
        }

        internal class NestedContentValidator : IValueValidator
        {
            private readonly PropertyEditorCollection _propertyEditors;

            public NestedContentValidator(PropertyEditorCollection propertyEditors)
            {
                _propertyEditors = propertyEditors;
            }

            public IEnumerable<ValidationResult> Validate(object rawValue, string valueType, object dataTypeConfiguration)
            {
                if (rawValue == null)
                    yield break;

                var value = JsonConvert.DeserializeObject<List<object>>(rawValue.ToString());
                if (value == null)
                    yield break;

                var dataTypeService = Current.Services.DataTypeService;
                for (var i = 0; i < value.Count; i++)
                {
                    var o = value[i];
                    var propValues = (JObject) o;

                    var contentType = GetElementType(propValues);
                    if (contentType == null) continue;

                    var propValueKeys = propValues.Properties().Select(x => x.Name).ToArray();

                    foreach (var propKey in propValueKeys)
                    {
                        var propType = contentType.CompositionPropertyTypes.FirstOrDefault(x => x.Alias == propKey);
                        if (propType != null)
                        {
                            var config = dataTypeService.GetDataType(propType.DataTypeId).Configuration;
                            var propertyEditor = _propertyEditors[propType.PropertyEditorAlias];

                            foreach (var validator in propertyEditor.GetValueEditor().Validators)
                            {
                                foreach (var result in validator.Validate(propValues[propKey], propertyEditor.GetValueEditor().ValueType, config))
                                {
                                    result.ErrorMessage = "Item " + (i + 1) + " '" + propType.Name + "' " + result.ErrorMessage;
                                    yield return result;
                                }
                            }

                            // Check mandatory
                            if (propType.Mandatory)
                            {
                                if (propValues[propKey] == null)
                                    yield return new ValidationResult("Item " + (i + 1) + " '" + propType.Name + "' cannot be null", new[] { propKey });
                                else if (propValues[propKey].ToString().IsNullOrWhiteSpace() || (propValues[propKey].Type == JTokenType.Array && !propValues[propKey].HasValues))
                                    yield return new ValidationResult("Item " + (i + 1) + " '" + propType.Name + "' cannot be empty", new[] { propKey });
                            }

                            // Check regex
                            if (!propType.ValidationRegExp.IsNullOrWhiteSpace()
                                && propValues[propKey] != null && !propValues[propKey].ToString().IsNullOrWhiteSpace())
                            {
                                var regex = new Regex(propType.ValidationRegExp);
                                if (!regex.IsMatch(propValues[propKey].ToString()))
                                {
                                    yield return new ValidationResult("Item " + (i + 1) + " '" + propType.Name + "' is invalid, it does not match the correct pattern", new[] { propKey });
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        private static bool IsSystemPropertyKey(string propKey)
        {
            return propKey == "name" || propKey == "key" || propKey == ContentTypeAliasPropertyKey;
        }
    }
}
