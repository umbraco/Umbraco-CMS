using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Umbraco.Extensions;

public static class ModelStateExtensions
{
    /// <summary>
    ///     Checks if there are any model errors on any fields containing the prefix
    /// </summary>
    /// <param name="state"></param>
    /// <param name="prefix"></param>
    /// <returns></returns>
    public static bool IsValid(this ModelStateDictionary state, string prefix) =>
        state.Where(v => v.Key.StartsWith(prefix + ".")).All(v => !v.Value?.Errors.Any() ?? false);

    public static IDictionary<string, object> ToErrorDictionary(this ModelStateDictionary modelState)
    {
        var modelStateError = new Dictionary<string, object>();
        foreach (KeyValuePair<string, ModelStateEntry> keyModelStatePair in modelState)
        {
            var key = keyModelStatePair.Key;
            ModelErrorCollection errors = keyModelStatePair.Value.Errors;
            if (errors.Count > 0)
            {
                modelStateError.Add(key, errors.Select(error => error.ErrorMessage));
            }
        }

        return modelStateError;
    }

    /// <summary>
    ///     Serializes the ModelState to JSON for JavaScript to interrogate the errors
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public static JsonResult ToJsonErrors(this ModelStateDictionary state) =>
        new(new
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
                                           e.Value.Errors.Where(x => x.Exception != null).Select(x => x.Exception!.Message)),
                               },
        });
}
