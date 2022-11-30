using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Umbraco.Cms.Web.BackOffice.PropertyEditors.Validation;

namespace Umbraco.Extensions;

public static class ModelStateExtensions
{
    /// <summary>
    ///     Adds the <see cref="ValidationResult" /> to the model state with the appropriate keys for property errors
    /// </summary>
    /// <param name="modelState"></param>
    /// <param name="result"></param>
    /// <param name="propertyAlias"></param>
    /// <param name="culture"></param>
    /// <param name="segment"></param>
    internal static void AddPropertyValidationError(this ModelStateDictionary modelState,
        ValidationResult result,
        string propertyAlias,
        string culture = "",
        string segment = "") =>
        modelState.AddValidationError(
            result,
            "_Properties",
            propertyAlias,
            //if the culture is null, we'll add the term 'invariant' as part of the key
            culture.IsNullOrWhiteSpace() ? "invariant" : culture,
            // if the segment is null, we'll add the term 'null' as part of the key
            segment.IsNullOrWhiteSpace() ? "null" : segment);

    /// <summary>
    ///     Adds an <see cref="ContentPropertyValidationResult" /> error to model state for a property so we can use it on the
    ///     client side.
    /// </summary>
    /// <param name="modelState"></param>
    /// <param name="result"></param>
    /// <param name="propertyAlias"></param>
    /// <param name="culture">The culture for the property, if the property is invariant than this is empty</param>
    internal static void AddPropertyError(this ModelStateDictionary modelState,
        ValidationResult result, string propertyAlias, string culture = "", string segment = "") =>
        modelState.AddPropertyValidationError(new ContentPropertyValidationResult(result, culture, segment),
            propertyAlias, culture, segment);

    /// <summary>
    ///     Adds a generic culture error for use in displaying the culture validation error in the save/publish/etc... dialogs
    /// </summary>
    /// <param name="modelState"></param>
    /// <param name="culture"></param>
    /// <param name="segment"></param>
    /// <param name="errMsg"></param>
    internal static void AddVariantValidationError(this ModelStateDictionary modelState,
        string? culture, string? segment, string errMsg)
    {
        var key = "_content_variant_" + (culture.IsNullOrWhiteSpace() ? "invariant" : culture) + "_" +
                  (segment.IsNullOrWhiteSpace() ? "null" : segment) + "_";
        if (modelState.ContainsKey(key))
        {
            return;
        }

        modelState.AddModelError(key, errMsg);
    }

    /// <summary>
    ///     Returns a list of cultures that have property validation errors
    /// </summary>
    /// <param name="modelState"></param>
    /// <param name="localizationService"></param>
    /// <param name="cultureForInvariantErrors">The culture to affiliate invariant errors with</param>
    /// <returns>
    ///     A list of cultures that have property validation errors. The default culture will be returned for any invariant
    ///     property errors.
    /// </returns>
    internal static IReadOnlyList<(string? culture, string? segment)>? GetVariantsWithPropertyErrors(
        this ModelStateDictionary modelState,
        string? cultureForInvariantErrors)
    {
        //Add any variant specific errors here
        var variantErrors = modelState.Keys
            .Where(key => key.StartsWith("_Properties.")) //only choose _Properties errors
            .Select(x => x.Split('.')) //split into parts
            .Where(x => x.Length >= 4 && !x[2].IsNullOrWhiteSpace() && !x[3].IsNullOrWhiteSpace())
            .Select(x => (culture: x[2], segment: x[3]))
            //if the culture is marked "invariant" than return the default language, this is because we can only edit invariant properties on the default language
            //so errors for those must show up under the default lang.
            //if the segment is marked "null" then return an actual null
            .Select(x =>
            {
                var culture = x.culture == "invariant" ? cultureForInvariantErrors : x.culture;
                var segment = x.segment == "null" ? null : x.segment;
                return (culture, segment);
            })
            .Distinct()
            .ToList();

        return variantErrors;
    }

    /// <summary>
    ///     Returns a list of cultures that have any validation errors
    /// </summary>
    /// <param name="modelState"></param>
    /// <param name="localizationService"></param>
    /// <param name="cultureForInvariantErrors">The culture to affiliate invariant errors with</param>
    /// <returns>
    ///     A list of cultures that have validation errors. The default culture will be returned for any invariant errors.
    /// </returns>
    internal static IReadOnlyList<(string? culture, string? segment)>? GetVariantsWithErrors(
        this ModelStateDictionary modelState, string? cultureForInvariantErrors)
    {
        IReadOnlyList<(string? culture, string? segment)>? propertyVariantErrors =
            modelState.GetVariantsWithPropertyErrors(cultureForInvariantErrors);

        //now check the other special variant errors that are
        IEnumerable<(string? culture, string? segment)>? genericVariantErrors = modelState.Keys
            .Where(x => x.StartsWith("_content_variant_") && x.EndsWith("_"))
            .Select(x => x.TrimStart("_content_variant_").TrimEnd("_"))
            .Select(x =>
            {
                // Format "<culture>_<segment>"
                var cs = x.Split(new[] { '_' });
                return (culture: cs[0], segment: cs[1]);
            })
            .Where(x => !x.culture.IsNullOrWhiteSpace())
            //if it's marked "invariant" than return the default language, this is because we can only edit invariant properties on the default language
            //so errors for those must show up under the default lang.
            //if the segment is marked "null" then return an actual null
            .Select(x =>
            {
                var culture = x.culture == "invariant" ? cultureForInvariantErrors : x.culture;
                var segment = x.segment == "null" ? null : x.segment;
                return (culture, segment);
            })
            .Distinct();

        return propertyVariantErrors?.Union(genericVariantErrors).Distinct().ToList();
    }

    /// <summary>
    ///     Adds the error to model state correctly for a property so we can use it on the client side.
    /// </summary>
    /// <param name="modelState"></param>
    /// <param name="result"></param>
    /// <param name="parts">
    ///     Each model state validation error has a name and in most cases this name is made up of parts which are delimited by
    ///     a '.'
    /// </param>
    internal static void AddValidationError(this ModelStateDictionary modelState,
        ValidationResult result, params string[] parts)
    {
        // if there are assigned member names, we combine the member name with the owner name
        // so that we can try to match it up to a real field. otherwise, we assume that the
        // validation message is for the overall owner.
        // Owner = the component being validated, like a content property but could be just an HTML field on another editor

        var withNames = false;
        var delimitedParts = string.Join(".", parts);
        foreach (var memberName in result.MemberNames)
        {
            modelState.TryAddModelError($"{delimitedParts}.{memberName}", result.ErrorMessage ?? string.Empty);
            withNames = true;
        }

        if (!withNames)
        {
            modelState.TryAddModelError($"{delimitedParts}", result.ToString());
        }
    }
}
