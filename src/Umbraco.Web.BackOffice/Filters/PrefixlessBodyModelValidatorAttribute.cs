using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Umbraco.Cms.Web.BackOffice.Filters;

/// <summary>
///     Applying this attribute to any controller will ensure that the parameter name (prefix) is not part of the
///     validation error keys.
/// </summary>
public class PrefixlessBodyModelValidatorAttribute : TypeFilterAttribute
{
    //TODO: Could be a better solution to replace the IModelValidatorProvider and ensure the errors are created
    //without the prefix, instead of removing it afterwards. But I couldn't find any way to do this for only some
    //of the controllers. IObjectModelValidator seems to be the interface to implement and replace in the container
    //to handle it for the entire solution.
    public PrefixlessBodyModelValidatorAttribute() : base(typeof(PrefixlessBodyModelValidatorFilter))
    {
    }

    private class PrefixlessBodyModelValidatorFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.IsValid)
            {
                return;
            }

            //Remove prefix from errors
            foreach (KeyValuePair<string, ModelStateEntry> modelStateItem in context.ModelState)
            {
                foreach (var prefix in context.ActionArguments.Keys)
                {
                    if (modelStateItem.Key.StartsWith(prefix))
                    {
                        if (modelStateItem.Value.Errors.Any())
                        {
                            var newKey = modelStateItem.Key.Substring(prefix.Length).TrimStart('.');
                            foreach (ModelError valueError in modelStateItem.Value.Errors)
                            {
                                context.ModelState.TryAddModelError(newKey, valueError.ErrorMessage);
                            }

                            context.ModelState.Remove(modelStateItem.Key);
                        }
                    }
                }
            }
        }
    }
}
