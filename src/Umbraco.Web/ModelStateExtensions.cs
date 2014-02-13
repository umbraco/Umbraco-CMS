using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

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


		//NOTE: we used this alot in v5 when we had editors in MVC, this was really handy for knockout editors using JS

		///// <summary>
		///// Adds an error to the model state that has to do with data validation, this is generally used for JSON responses
		///// </summary>
		///// <param name="state"></param>
		///// <param name="errorMessage"></param>
		//public static void AddDataValidationError(this ModelStateDictionary state, string errorMessage)
		//{
		//    state.AddModelError("DataValidation", errorMessage);
		//}

        /// <summary>
        /// Adds the error to model state correctly for a property so we can use it on the client side.
        /// </summary>
        /// <param name="modelState"></param>
        /// <param name="result"></param>
        /// <param name="propertyAlias"></param>
        internal static void AddPropertyError(this System.Web.Http.ModelBinding.ModelStateDictionary modelState, ValidationResult result, string propertyAlias)
        {
            //if there are no member names supplied then we assume that the validation message is for the overall property
            // not a sub field on the property editor
            if (!result.MemberNames.Any())
            {
                //add a model state error for the entire property
                modelState.AddModelError(string.Format("{0}.{1}", "Properties", propertyAlias), result.ErrorMessage);
            }
            else
            {
                //there's assigned field names so we'll combine the field name with the property name
                // so that we can try to match it up to a real sub field of this editor
                foreach (var field in result.MemberNames)
                {
                    modelState.AddModelError(string.Format("{0}.{1}.{2}", "Properties", propertyAlias, field), result.ErrorMessage);
                }
            }
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