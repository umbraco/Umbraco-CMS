using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
                // TODO: Change to the new validation structure


                var jo = new JObject();
                // recurse to write out an array of ValidationResultCollection
                var obj = JArray.FromObject(nestedResult.ValidationResults, camelCaseSerializer);
                jo.Add("nestedValidation", obj);
                jo.WriteTo(writer);
            }
            else if (validationResult is ComplexEditorElementTypeValidationResult elementTypeValidationResult && elementTypeValidationResult.ValidationResults.Count > 0)
            {
                var joElementType = new JObject();
                var joPropertyType = new JObject();
                // loop over property validations
                foreach (var propTypeResult in elementTypeValidationResult.ValidationResults)
                {

                    // TODO: I think here we could do the ModelState thing? instead of recursing? We'd just have to
                    // not recurse if it was the exact type of the base class ValidationResult and build up the ModelState values
                    var ja = new JArray();
                    foreach (var result in propTypeResult.ValidationResults)
                    {
                        // recurse to get the validation result object and add to the array
                        var obj = JObject.FromObject(result, camelCaseSerializer);
                        ja.Add(obj);
                    }
                    // create a dictionary entry 
                    joPropertyType.Add(propTypeResult.PropertyTypeAlias, ja);
                }
                joElementType.Add(elementTypeValidationResult.ElementTypeAlias, joPropertyType);
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
