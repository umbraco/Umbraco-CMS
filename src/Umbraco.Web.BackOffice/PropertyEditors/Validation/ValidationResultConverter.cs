using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Umbraco.Cms.Core.PropertyEditors.Validation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.PropertyEditors.Validation;

/// <summary>
///     Custom json converter for <see cref="ValidationResult" /> and <see cref="ContentPropertyValidationResult" />
/// </summary>
/// <remarks>
///     This converter is specifically used to convert validation results for content in order to be able to have nested
///     validation results for complex editors.
///     For a more indepth explanation of how server side validation works with the angular app, see this GitHub PR:
///     https://github.com/umbraco/Umbraco-CMS/pull/8339
/// </remarks>
internal class ValidationResultConverter : JsonConverter
{
    private readonly string _culture;
    private readonly string _segment;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="culture">The culture of the containing property which will be transfered to all child model state</param>
    /// <param name="segment">The segment of the containing property which will be transfered to all child model state</param>
    public ValidationResultConverter(string culture = "", string segment = "")
    {
        _culture = culture;
        _segment = segment;
    }

    public override bool CanConvert(Type objectType) => typeof(ValidationResult).IsAssignableFrom(objectType);

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) => throw new NotImplementedException();

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        var camelCaseSerializer = new JsonSerializer
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            DefaultValueHandling = DefaultValueHandling.Ignore
        };
        foreach (JsonConverter c in serializer.Converters)
        {
            camelCaseSerializer.Converters.Add(c);
        }

        var validationResult = (ValidationResult?)value;

        if (validationResult is ComplexEditorValidationResult nestedResult && nestedResult.ValidationResults.Count > 0)
        {
            var ja = new JArray();
            foreach (ComplexEditorElementTypeValidationResult nested in nestedResult.ValidationResults)
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
        else if (validationResult is ComplexEditorElementTypeValidationResult elementTypeValidationResult &&
                 elementTypeValidationResult.ValidationResults.Count > 0)
        {
            var joElementType = new JObject
            {
                {"$id", elementTypeValidationResult.BlockId},

                // We don't use this anywhere, though it's nice for debugging
                {"$elementTypeAlias", elementTypeValidationResult.ElementTypeAlias}
            };

            var modelState = new ModelStateDictionary();

            // loop over property validations
            foreach (ComplexEditorPropertyTypeValidationResult propTypeResult in elementTypeValidationResult
                         .ValidationResults)
            {
                // group the results by their type and iterate the groups
                foreach (IGrouping<Type, ValidationResult> result in propTypeResult.ValidationResults.GroupBy(x =>
                             x.GetType()))
                {
                    // if the group's type isn't ComplexEditorValidationResult then it will in 99% of cases be
                    // just ValidationResult for whcih we want to create the sub "ModelState" data. If it's not a normal
                    // ValidationResult it will still just be converted to normal ModelState.

                    if (result.Key == typeof(ComplexEditorValidationResult))
                    {
                        // if it's ComplexEditorValidationResult then there can only be one which is validated so just get the single
                        if (result.Any())
                        {
                            ValidationResult complexResult = result.Single();
                            // recurse to get the validation result object
                            var obj = JToken.FromObject(complexResult, camelCaseSerializer);
                            joElementType.Add(propTypeResult.PropertyTypeAlias, obj);

                            // For any nested property error we add the model state as empty state for that nested property
                            // NOTE: Instead of the empty validation message we could put in the translated
                            // "errors/propertyHasErrors" message, however I think that leaves for less flexibility since it could/should be
                            // up to the front-end validator to show whatever message it wants (if any) for an error indicating a nested property error.
                            // Will leave blank.
                            modelState.AddPropertyValidationError(new ValidationResult(string.Empty), propTypeResult.PropertyTypeAlias, _culture, _segment);
                        }
                    }
                    else
                    {
                        foreach (ValidationResult v in result)
                        {
                            modelState.AddPropertyValidationError(v, propTypeResult.PropertyTypeAlias, _culture, _segment);
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
            if (!validationResult?.ErrorMessage.IsNullOrWhiteSpace() ?? false)
            {
                var errObj = JToken.FromObject(validationResult!.ErrorMessage!, camelCaseSerializer);
                jo.Add("errorMessage", errObj);
            }

            if (validationResult?.MemberNames.Any() ?? false)
            {
                var memberNamesObj = JToken.FromObject(validationResult.MemberNames, camelCaseSerializer);
                jo.Add("memberNames", memberNamesObj);
            }

            if (jo.HasValues)
            {
                jo.WriteTo(writer);
            }
        }
    }
}
