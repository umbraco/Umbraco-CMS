using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.NestedContentAlias, "Nested Content", "nestedcontent", ValueType = "JSON", Group = "lists", Icon = "icon-thumbnail-list")]
    public class NestedContentPropertyEditor : PropertyEditor
    {
        internal const string ContentTypeAliasPropertyKey = "ncContentTypeAlias";

        private IDictionary<string, object> _defaultPreValues;
        public override IDictionary<string, object> DefaultPreValues
        {
            get { return _defaultPreValues; }
            set { _defaultPreValues = value; }
        }

        public NestedContentPropertyEditor()
        {
            // Setup default values
            _defaultPreValues = new Dictionary<string, object>
            {
                {NestedContentPreValueEditor.ContentTypesPreValueKey, ""},
                {"minItems", 0},
                {"maxItems", 0},
                {"confirmDeletes", "1"},
                {"showIcons", "1"}
            };
        }

        #region Pre Value Editor

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new NestedContentPreValueEditor();
        }

        internal class NestedContentPreValueEditor : PreValueEditor
        {
            internal const string ContentTypesPreValueKey = "contentTypes";

            [PreValueField(ContentTypesPreValueKey, "Doc Types", "views/propertyeditors/nestedcontent/nestedcontent.doctypepicker.html", Description = "Select the doc types to use as the data blueprint.")]
            public string[] ContentTypes { get; set; }

            [PreValueField("minItems", "Min Items", "number", Description = "Set the minimum number of items allowed.")]
            public string MinItems { get; set; }

            [PreValueField("maxItems", "Max Items", "number", Description = "Set the maximum number of items allowed.")]
            public string MaxItems { get; set; }

            [PreValueField("confirmDeletes", "Confirm Deletes", "boolean", Description = "Set whether item deletions should require confirming.")]
            public string ConfirmDeletes { get; set; }

            [PreValueField("showIcons", "Show Icons", "boolean", Description = "Set whether to show the items doc type icon in the list.")]
            public string ShowIcons { get; set; }

            [PreValueField("hideLabel", "Hide Label", "boolean", Description = "Set whether to hide the editor label and have the list take up the full width of the editor window.")]
            public string HideLabel { get; set; }

            public override IDictionary<string, object> ConvertDbToEditor(IDictionary<string, object> defaultPreVals, PreValueCollection persistedPreVals)
            {
                // re-format old style (v0.1.1) pre values if necessary
                NestedContentHelper.ConvertPreValueCollectionFromV011(persistedPreVals);

                return base.ConvertDbToEditor(defaultPreVals, persistedPreVals);
            }
        }

        #endregion

        #region Value Editor

        protected override PropertyValueEditor CreateValueEditor()
        {
            return new NestedContentPropertyValueEditor(base.CreateValueEditor());
        }

        internal class NestedContentPropertyValueEditor : PropertyValueEditorWrapper
        {
            public NestedContentPropertyValueEditor(PropertyValueEditor wrapped)
                : base(wrapped)
            {
                Validators.Add(new NestedContentValidator());
            }

            internal ServiceContext Services
            {
                get { return ApplicationContext.Current.Services; }
            }

            public override void ConfigureForDisplay(PreValueCollection preValues)
            {
                base.ConfigureForDisplay(preValues);

                if (preValues.PreValuesAsDictionary.ContainsKey("hideLabel"))
                {
                    var boolAttempt = preValues.PreValuesAsDictionary["hideLabel"].Value.TryConvertTo<bool>();
                    if (boolAttempt.Success)
                    {
                        HideLabel = boolAttempt.Result;
                    }
                }
            }

            #region DB to String

            public override string ConvertDbToString(Property property, PropertyType propertyType, IDataTypeService dataTypeService)
            {
                // Convert / validate value
                if (property.Value == null)
                    return string.Empty;

                var propertyValue = property.Value.ToString();
                if (string.IsNullOrWhiteSpace(propertyValue))
                    return string.Empty;

                var value = JsonConvert.DeserializeObject<List<object>>(propertyValue);
                if (value == null)
                    return string.Empty;

                // Process value
                PreValueCollection preValues = null;
                for (var i = 0; i < value.Count; i++)
                {
                    var o = value[i];
                    var propValues = ((JObject)o);

                    // convert from old style (v0.1.1) data format if necessary
                    NestedContentHelper.ConvertItemValueFromV011(propValues, propertyType.DataTypeDefinitionId, ref preValues);

                    var contentType = NestedContentHelper.GetContentTypeFromItem(propValues);
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
                            try
                            {
                                // Create a fake property using the property abd stored value
                                var prop = new Property(propType, propValues[propKey] == null ? null : propValues[propKey].ToString());

                                // Lookup the property editor
                                var propEditor = PropertyEditorResolver.Current.GetByAlias(propType.PropertyEditorAlias);

                                // Get the editor to do it's conversion, and store it back
                                propValues[propKey] = propEditor.ValueEditor.ConvertDbToString(prop, propType, dataTypeService);
                            }
                            catch (InvalidOperationException)
                            {
                                // https://github.com/umco/umbraco-nested-content/issues/111
                                // Catch any invalid cast operations as likely means courier failed due to missing
                                // or trashed item so couldn't convert a guid back to an int

                                propValues[propKey] = null;
                            }
                        }

                    }
                }

                // Return the serialized value
                return JsonConvert.SerializeObject(value);
            }

            #endregion

            #region DB to Editor

            public override object ConvertDbToEditor(Property property, PropertyType propertyType, IDataTypeService dataTypeService)
            {
                if (property.Value == null)
                    return string.Empty;

                var propertyValue = property.Value.ToString();
                if (string.IsNullOrWhiteSpace(propertyValue))
                    return string.Empty;

                var value = JsonConvert.DeserializeObject<List<object>>(propertyValue);
                if (value == null)
                    return string.Empty;

                // Process value
                PreValueCollection preValues = null;
                for (var i = 0; i < value.Count; i++)
                {
                    var o = value[i];
                    var propValues = ((JObject)o);

                    // convert from old style (v0.1.1) data format if necessary
                    NestedContentHelper.ConvertItemValueFromV011(propValues, propertyType.DataTypeDefinitionId, ref preValues);

                    var contentType = NestedContentHelper.GetContentTypeFromItem(propValues);
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
                            try
                            {
                                // Create a fake property using the property and stored value
                                var prop = new Property(propType, propValues[propKey] == null ? null : propValues[propKey].ToString());

                                // Lookup the property editor
                                var propEditor = PropertyEditorResolver.Current.GetByAlias(propType.PropertyEditorAlias);

                                // Get the editor to do it's conversion
                                var newValue = propEditor.ValueEditor.ConvertDbToEditor(prop, propType, dataTypeService);

                                // Store the value back
                                propValues[propKey] = (newValue == null) ? null : JToken.FromObject(newValue);
                            }
                            catch (InvalidOperationException)
                            {
                                // https://github.com/umco/umbraco-nested-content/issues/111
                                // Catch any invalid cast operations as likely means courier failed due to missing
                                // or trashed item so couldn't convert a guid back to an int

                                propValues[propKey] = null;
                            }
                        }

                    }
                }

                // Return the strongly-typed object, Umbraco will handle the JSON serializing/parsing, then Angular can handle it directly
                return value;
            }

            #endregion

            #region Editor to DB

            public override object ConvertEditorToDb(ContentPropertyData editorValue, object currentValue)
            {
                if (editorValue.Value == null)
                    return null;

                var rawValue = editorValue.Value.ToString();
                if (string.IsNullOrWhiteSpace(rawValue))
                    return null;

                var value = JsonConvert.DeserializeObject<List<object>>(rawValue);
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

                    var contentType = NestedContentHelper.GetContentTypeFromItem(propValues);
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
                            var propPreValues = Services.DataTypeService.GetPreValuesCollectionByDataTypeId(
                                propType.DataTypeDefinitionId);

                            // Lookup the property editor
                            var propEditor = PropertyEditorResolver.Current.GetByAlias(propType.PropertyEditorAlias);

                            // Create a fake content property data object
                            var contentPropData = new ContentPropertyData(
                                propValues[propKey], propPreValues,
                                new Dictionary<string, object>());

                            // Get the property editor to do it's conversion
                            var newValue = propEditor.ValueEditor.ConvertEditorToDb(contentPropData, propValues[propKey]);

                            // Store the value back
                            propValues[propKey] = (newValue == null) ? null : JToken.FromObject(newValue);
                        }

                    }
                }

                return JsonConvert.SerializeObject(value);
            }

            #endregion
        }

        internal class NestedContentValidator : IPropertyValidator
        {
            public IEnumerable<ValidationResult> Validate(object rawValue, PreValueCollection preValues, PropertyEditor editor)
            {
                var value = JsonConvert.DeserializeObject<List<object>>(rawValue.ToString());
                if (value == null)
                    yield break;

                IDataTypeService dataTypeService = ApplicationContext.Current.Services.DataTypeService;
                for (var i = 0; i < value.Count; i++)
                {
                    var o = value[i];
                    var propValues = ((JObject)o);

                    var contentType = NestedContentHelper.GetContentTypeFromItem(propValues);
                    if (contentType == null)
                    {
                        continue;
                    }

                    var propValueKeys = propValues.Properties().Select(x => x.Name).ToArray();

                    foreach (var propKey in propValueKeys)
                    {
                        var propType = contentType.CompositionPropertyTypes.FirstOrDefault(x => x.Alias == propKey);
                        if (propType != null)
                        {
                            PreValueCollection propPrevalues = dataTypeService.GetPreValuesCollectionByDataTypeId(propType.DataTypeDefinitionId);
                            PropertyEditor propertyEditor = PropertyEditorResolver.Current.GetByAlias(propType.PropertyEditorAlias);

                            foreach (IPropertyValidator validator in propertyEditor.ValueEditor.Validators)
                            {
                                foreach (ValidationResult result in validator.Validate(propValues[propKey], propPrevalues, propertyEditor))
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
