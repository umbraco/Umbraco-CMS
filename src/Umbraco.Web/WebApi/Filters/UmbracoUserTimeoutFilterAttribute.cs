using System.Globalization;
using System.Web.Http.Filters;
using Umbraco.Core.Security;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// This will check if the request is authenticated and if there's an auth ticket present we will 
    /// add a custom header to the response indicating how many seconds are remaining for the current 
    /// user's session. This allows us to keep track of a user's session effectively in the back office.
    /// </summary>
    public sealed class UmbracoUserTimeoutFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);

            //this can occur if an error has already occurred.
            if (actionExecutedContext.Response == null) return;

            var httpContextAttempt = actionExecutedContext.Request.TryGetHttpContext();
            if (httpContextAttempt.Success)
            {
                var ticket = httpContextAttempt.Result.GetUmbracoAuthTicket();
                if (ticket != null && ticket.Expired == false)
                {
                    var remainingSeconds = httpContextAttempt.Result.GetRemainingAuthSeconds();
                    actionExecutedContext.Response.Headers.Add("X-Umb-User-Seconds", remainingSeconds.ToString(CultureInfo.InvariantCulture));
                }
            }
        }
    }
}