using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Umbraco.Web.BackOffice.Filters
{
    public class PrefixlessBodyModelValidatorAttribute : TypeFilterAttribute
    {
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
                if (context.ModelState.IsValid) return;

                //Remove prefix from errors


                foreach (var modelStateItem in context.ModelState)
                {
                    foreach (var prefix in context.ActionArguments.Keys)
                    {
                        if (modelStateItem.Key.StartsWith(prefix))
                        {
                            if (modelStateItem.Value.Errors.Any())
                            {

                                var newKey = modelStateItem.Key.Substring(prefix.Length).TrimStart('.');
                                foreach (var valueError in modelStateItem.Value.Errors)
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
}
