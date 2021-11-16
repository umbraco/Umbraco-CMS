using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.PropertyEditors.Validation;

namespace Umbraco.Web
{
    internal static class ModelStateExtensions
    {

        /// <summary>
        /// Merges ModelState that has names matching the prefix
        /// </summary>
        /// <param name="state"></param>
        /// <param name="dictionary"></param>
        /// <param name="prefix"></param>
        public static void Merge(this ModelStateDictionary state, ModelStateDictionary dictionary, string prefix)
        {
            if (dictionary == null)
                return;
            foreach (var (key, value) in dictionary
                //It can either equal the prefix exactly (model level errors) or start with the prefix. (property level errors)
                .Where(keyValuePair => keyValuePair.Key == prefix || keyValuePair.Key.StartsWith(prefix + ".")))
            {
                state[key] = value;
            }
        }

        /// <summary>
        /// Checks if there are any model errors on any fields containing the prefix
        /// </summary>
        /// <param name="state"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static bool IsValid(this ModelStateDictionary state, string prefix)
        {
            return state.Where(v => v.Key.StartsWith(prefix + ".")).All(v => !v.Value.Errors.Any());
        }

        /// <summary>
        /// Adds an <see cref="ContentPropertyValidationResult"/> error to model state for a property so we can use it on the client side.
        /// </summary>
        /// <param name="modelState"></param>
        /// <param name="result"></param>
        /// <param name="propertyAlias"></param>
        /// <param name="culture">The culture for the property, if the property is invariant than this is empty</param>
        internal static void AddPropertyError(this System.Web.Http.ModelBinding.ModelStateDictionary modelState,
            ValidationResult result, string propertyAlias, string culture = "", string segment = "")
        {
            modelState.AddPropertyValidationError(new ContentPropertyValidationResult(result, culture, segment), propertyAlias, culture, segment);
        }

        /// <summary>
        /// Adds the <see cref="ValidationResult"/> to the model state with the appropriate keys for property errors
        /// </summary>
        /// <param name="modelState"></param>
        /// <param name="result"></param>
        /// <param name="propertyAlias"></param>
        /// <param name="culture"></param>
        /// <param name="segment"></param>
        internal static void AddPropertyValidationError(this System.Web.Http.ModelBinding.ModelStateDictionary modelState,
            ValidationResult result, string propertyAlias, string culture = "", string segment = "")
        {
            modelState.AddValidationError(
                result,
                "_Properties",
                propertyAlias,
                //if the culture is null, we'll add the term 'invariant' as part of the key
                culture.IsNullOrWhiteSpace() ? "invariant" : culture,
                // if the segment is null, we'll add the term 'null' as part of the key
                segment.IsNullOrWhiteSpace() ? "null" : segment);
        }

        /// <summary>
        /// Adds a generic culture error for use in displaying the culture validation error in the save/publish/etc... dialogs
        /// </summary>
        /// <param name="modelState"></param>
        /// <param name="culture"></param>
        /// <param name="segment"></param>
        /// <param name="errMsg"></param>
        internal static void AddVariantValidationError(this System.Web.Http.ModelBinding.ModelStateDictionary modelState,
            string culture, string segment, string errMsg)
        {
            var key = "_content_variant_" + (culture.IsNullOrWhiteSpace() ? "invariant" : culture) + "_" + (segment.IsNullOrWhiteSpace() ? "null" : segment) + "_";
            if (modelState.ContainsKey(key)) return;
            modelState.AddModelError(key, errMsg);
        }

        /// <summary>
        /// Returns a list of cultures that have property validation errors
        /// </summary>
        /// <param name="modelState"></param>
        /// <param name="localizationService"></param>
        /// <param name="cultureForInvariantErrors">The culture to affiliate invariant errors with</param>
        /// <returns>
        /// A list of cultures that have property validation errors. The default culture will be returned for any invariant property errors.
        /// </returns>
        internal static IReadOnlyList<(string culture, string segment)> GetVariantsWithPropertyErrors(this System.Web.Http.ModelBinding.ModelStateDictionary modelState,
            string cultureForInvariantErrors)
        {
            //Add any variant specific errors here
            var variantErrors = modelState.Keys
                .Where(key => key.StartsWith("_Properties.")) //only choose _Properties errors
                .Select(x => x.Split(Constants.CharArrays.Period)) //split into parts
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
        /// Returns a list of cultures that have any validation errors
        /// </summary>
        /// <param name="modelState"></param>
        /// <param name="localizationService"></param>
        /// <param name="cultureForInvariantErrors">The culture to affiliate invariant errors with</param>
        /// <returns>
        /// A list of cultures that have validation errors. The default culture will be returned for any invariant errors.
        /// </returns>
        internal static IReadOnlyList<(string culture, string segment)> GetVariantsWithErrors(this System.Web.Http.ModelBinding.ModelStateDictionary modelState, string cultureForInvariantErrors)
        {
            var propertyVariantErrors = modelState.GetVariantsWithPropertyErrors(cultureForInvariantErrors);

            //now check the other special variant errors that are
            var genericVariantErrors = modelState.Keys
                .Where(x => x.StartsWith("_content_variant_") && x.EndsWith("_"))
                .Select(x => x.TrimStart("_content_variant_").TrimEnd("_"))                
                .Select(x =>
                {
                    // Format "<culture>_<segment>"
                    var cs = x.Split(Constants.CharArrays.Underscore);
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

            return propertyVariantErrors.Union(genericVariantErrors).Distinct().ToList();
        }
        
        /// <summary>
        /// Adds the error to model state correctly for a property so we can use it on the client side.
        /// </summary>
        /// <param name="modelState"></param>
        /// <param name="result"></param>
        /// <param name="parts">
        /// Each model state validation error has a name and in most cases this name is made up of parts which are delimited by a '.'
        /// </param>
        internal static void AddValidationError(this System.Web.Http.ModelBinding.ModelStateDictionary modelState,
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
                modelState.TryAddModelError($"{delimitedParts}.{memberName}", result.ToString());
                withNames = true;
            }
            if (!withNames)
            {
                modelState.TryAddModelError($"{delimitedParts}", result.ToString());
            }
                
        }

        /// <summary>
        /// Will add an error to model state for a key if that key and error don't already exist
        /// </summary>
        /// <param name="modelState"></param>
        /// <param name="key"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        private static bool TryAddModelError(this System.Web.Http.ModelBinding.ModelStateDictionary modelState, string key, string errorMsg)
        {
            if (modelState.TryGetValue(key, out var errs))
            {
                foreach(var e in errs.Errors)
                    if (e.ErrorMessage == errorMsg) return false; //if this same error message exists for the same key, just exit
            }

            modelState.AddModelError(key, errorMsg);
            return true;
        }

        public static IDictionary<string, object> ToErrorDictionary(this System.Web.Http.ModelBinding.ModelStateDictionary modelState)
        {
            var modelStateError = new Dictionary<string, object>();
            foreach (var keyModelStatePair in modelState)
            {
                var key = keyModelStatePair.Key;
                var errors = keyModelStatePair.Value.Errors;
                if (errors != null && errors.Count > 0)
                {
                    modelStateError.Add(key, errors.Select(error => error.ErrorMessage));
                }
            }
            return modelStateError;
        }

        /// <summary>
        /// Serializes the ModelState to JSON for JavaScript to interrogate the errors
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static JsonResult ToJsonErrors(this ModelStateDictionary state)
        {
            return new JsonResult
                {
                    Data = new
                        {
                            success = state.IsValid.ToString().ToLower(),
                            failureType = "ValidationError",
                            validationErrors = from e in state
                                               where e.Value.Errors.Count > 0
                                               select new
                                                   {
                                                       name = e.Key,
                                                       errors = e.Value.Errors.Select(x => x.ErrorMessage)
                                                   .Concat(
                                                       e.Value.Errors.Where(x => x.Exception != null).Select(x => x.Exception.Message))
                                                   }
                        }
                };
        }

       
    }
}
