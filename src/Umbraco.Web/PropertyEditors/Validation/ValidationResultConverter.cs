using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Http.ModelBinding;
using Umbraco.Core;

namespace Umbraco.Web.PropertyEditors.Validation
{
    /// <summary>
    /// Custom json converter for <see cref="ValidationResult"/> and <see cref="ContentPropertyValidationResult"/>
    /// </summary>
    /// <remarks>
    /// This converter is specifically used to convert validation results for content in order to be able to have nested
    /// validation results for complex editors.
    /// </remarks>
    internal class ValidationResultConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => typeof(ValidationResult).IsAssignableFrom(objectType);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var camelCaseSerializer = new JsonSerializer
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
            foreach (var c in serializer.Converters)
                camelCaseSerializer.Converters.Add(c);

            var validationResult = (ValidationResult)value;

            if (validationResult is ComplexEditorValidationResult nestedResult && nestedResult.ValidationResults.Count > 0)
            {
                var ja = new JArray();
                foreach(var nested in nestedResult.ValidationResults)
                {
                    // recurse to write out the ComplexEditorElementTypeValidationResult
                    var block = JObject.FromObject(nested, camelCaseSerializer);
                    ja.Add(block);
                }
                if (nestedResult.ValidationResults.Count > 0)
                {
                    ja.WriteTo(writer);
                }
            }
            else if (validationResult is ComplexEditorElementTypeValidationResult elementTypeValidationResult && elementTypeValidationResult.ValidationResults.Count > 0)
            {
                var joElementType = new JObject
                {
                    { "$id", elementTypeValidationResult.BlockId },
                    { "$elementTypeAlias", elementTypeValidationResult.ElementTypeAlias }
                };

                var modelState = new ModelStateDictionary();

                // loop over property validations
                foreach (var propTypeResult in elementTypeValidationResult.ValidationResults)
                {
                    // group the results by their type and iterate the groups
                    foreach (var result in propTypeResult.ValidationResults.GroupBy(x => x.GetType()))
                    {
                        // if the group's type isn't ComplexEditorValidationResult then it will in 99% of cases be
                        // just ValidationResult for whcih we want to create the sub "ModelState" data. If it's not a normal
                        // ValidationResult it will still just be converted to normal ModelState.

                        if (result.Key == typeof(ComplexEditorValidationResult))
                        {
                            // if it's ComplexEditorValidationResult then there can only be one which is validated so just get the single
                            if (result.Any())
                            {
                                var complexResult = result.Single();
                                // recurse to get the validation result object 
                                var obj = JToken.FromObject(complexResult, camelCaseSerializer);
                                joElementType.Add(propTypeResult.PropertyTypeAlias, obj);
                            }
                        }
                        else
                        {
                            foreach (var v in result)
                            {
                                modelState.AddPropertyValidationError(v, propTypeResult.PropertyTypeAlias);
                            }
                        }
                    }
                }

                if (modelState.Count > 0)
                {
                    joElementType.Add("ModelState", JObject.FromObject(modelState.ToErrorDictionary()));
                }

                joElementType.WriteTo(writer);
            }
            else 
            {
                
                if (validationResult is ContentPropertyValidationResult propertyValidationResult
                    && propertyValidationResult.ComplexEditorResults?.ValidationResults.Count > 0)
                {
                    // recurse to write out the NestedValidationResults
                    var obj = JToken.FromObject(propertyValidationResult.ComplexEditorResults, camelCaseSerializer);
                    obj.WriteTo(writer);
                }

                var jo = new JObject();
                if (!validationResult.ErrorMessage.IsNullOrWhiteSpace())
                {
                    var errObj = JToken.FromObject(validationResult.ErrorMessage, camelCaseSerializer);
                    jo.Add("errorMessage", errObj);
                }
                if (validationResult.MemberNames.Any())
                {
                    var memberNamesObj = JToken.FromObject(validationResult.MemberNames, camelCaseSerializer);
                    jo.Add("memberNames", memberNamesObj);
                }
                if (jo.HasValues)
                    jo.WriteTo(writer);
            }
        }
    }
}
