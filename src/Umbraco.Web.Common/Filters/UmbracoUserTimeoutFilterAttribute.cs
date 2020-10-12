using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Extensions;

namespace Umbraco.Web.Common.Filters
{
    /// <summary>
    /// This will check if the request is authenticated and if there's an auth ticket present we will
    /// add a custom header to the response indicating how many seconds are remaining for the current
    /// user's session. This allows us to keep track of a user's session effectively in the back office.
    /// </summary>
    public class UmbracoUserTimeoutFilterAttribute : TypeFilterAttribute
    {
        public UmbracoUserTimeoutFilterAttribute() : base(typeof(UmbracoUserTimeoutFilter))
        {
        }

        private class UmbracoUserTimeoutFilter : IActionFilter
        {

            public void OnActionExecuted(ActionExecutedContext context)
            {
                //this can occur if an error has already occurred.
                if (context.HttpContext.Response is null) return;

                // Using the new way to GetRemainingAuthSeconds, which does not require you to get the ticket from the request
                var remainingSeconds = context.HttpContext.User.GetRemainingAuthSeconds();

                context.HttpContext.Response.Headers.Add("X-Umb-User-Seconds", remainingSeconds.ToString(CultureInfo.InvariantCulture));
            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                // Noop
            }
        }
    }
}
