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

            if (validationResult is NestedValidationResults nestedResult)
            {
                if (nestedResult.ValidationResults.Count > 0)
                {
                    var validationItems = JToken.FromObject(nestedResult.ValidationResults, camelCaseSerializer);
                    validationItems.WriteTo(writer);
                }
            }
            else
            {
                var jo = new JObject();

                if (!validationResult.ErrorMessage.IsNullOrWhiteSpace())
                {
                    jo.Add("errorMessage", JToken.FromObject(validationResult.ErrorMessage, camelCaseSerializer));
                }

                if (validationResult.MemberNames.Any())
                {
                    jo.Add("memberNames", JToken.FromObject(validationResult.MemberNames, camelCaseSerializer));
                }

                if (validationResult is PropertyValidationResult propertyValidationResult)
                {
                    if (propertyValidationResult.NestedResuls?.ValidationResults.Count > 0)
                    {
                        jo.Add("nestedValidation", JToken.FromObject(propertyValidationResult.NestedResuls, camelCaseSerializer));
                    }
                }

                jo.WriteTo(writer);
            }
        }
    }
}
