﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Core;

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
            foreach (var keyValuePair in dictionary
                //It can either equal the prefix exactly (model level errors) or start with the prefix. (property level errors)
                .Where(keyValuePair => keyValuePair.Key == prefix || keyValuePair.Key.StartsWith(prefix + ".")))
            {
                state[keyValuePair.Key] = keyValuePair.Value;
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
        /// Adds the error to model state correctly for a property so we can use it on the client side.
        /// </summary>
        /// <param name="modelState"></param>
        /// <param name="result"></param>
        /// <param name="propertyAlias"></param>
        /// <param name="culture">The culture for the property, if the property is invariant than this is empty</param>
        internal static void AddPropertyError(this System.Web.Http.ModelBinding.ModelStateDictionary modelState,
            ValidationResult result, string propertyAlias, string culture = "")
        {
            if (culture == null)
                culture = "";
            modelState.AddValidationError(result, "_Properties", propertyAlias, culture);
        }

        /// <summary>
        /// Returns a list of cultures that have property errors
        /// </summary>
        /// <param name="modelState"></param>
        /// <returns></returns>
        internal static IReadOnlyList<string> GetCulturesWithPropertyErrors(this System.Web.Http.ModelBinding.ModelStateDictionary modelState)
        {
            //Add any culture specific errors here
            var cultureErrors = modelState.Keys
                .Select(x => x.Split('.')) //split into parts
                .Where(x => x.Length >= 3 && x[0] == "_Properties") //only choose _Properties errors
                .Select(x => x[2]) //select the culture part
                .Where(x => !x.IsNullOrWhiteSpace()) //if it has a value
                .Distinct()
                .ToList();

            return cultureErrors;
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
                modelState.AddModelError($"{delimitedParts}.{memberName}", result.ErrorMessage);
                withNames = true;
            }
            if (!withNames)
                modelState.AddModelError($"{delimitedParts}", result.ErrorMessage);
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
