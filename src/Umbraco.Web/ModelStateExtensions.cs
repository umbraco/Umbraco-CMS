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
			foreach (var keyValuePair in dictionary.Where(keyValuePair => keyValuePair.Key.StartsWith(prefix + ".")))
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