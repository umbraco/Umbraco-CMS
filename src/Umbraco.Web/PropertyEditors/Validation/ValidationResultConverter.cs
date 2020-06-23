using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;

namespace Umbraco.Web.PropertyEditors.Validation
{
    /// <summary>
    /// Custom json converter for <see cref="ValidationResult"/> and <see cref="PropertyValidationResult"/>
    /// </summary>
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

            if (validationResult is NestedValidationResults nestedResult && nestedResult.ValidationResults.Count > 0)
            {
                var jo = new JObject();
                // recurse to write out an array of ValidationResultCollection
                var obj = JArray.FromObject(nestedResult.ValidationResults, camelCaseSerializer);
                jo.Add("nestedValidation", obj);
                jo.WriteTo(writer);
            }
            else if (validationResult is ElementTypeValidationResult elementTypeValidationResult && elementTypeValidationResult.ValidationResults.Count > 0)
            {
                var joElementType = new JObject();
                var joPropertyType = new JObject();
                // loop over property validations
                foreach (var propTypeResult in elementTypeValidationResult.ValidationResults)
                {                    
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
                
                if (validationResult is PropertyValidationResult propertyValidationResult
                    && propertyValidationResult.NestedResuls?.ValidationResults.Count > 0)
                {
                    // recurse to write out the NestedValidationResults
                    var obj = JToken.FromObject(propertyValidationResult.NestedResuls, camelCaseSerializer);
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
