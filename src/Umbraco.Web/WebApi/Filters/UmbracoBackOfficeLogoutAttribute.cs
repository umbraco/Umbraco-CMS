using System.Web.Http.Filters;
using Umbraco.Core.Security;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// A filter that is used to remove the authorization cookie for the current user when the request is successful
    /// </summary>
    /// <remarks>
    /// This is used so that we can log a user OUT in conjunction with using other filters that modify the cookies collection.
    /// SD: I beleive this is a bug with web api since if you modify the cookies collection on the HttpContext.Current and then 
    /// use a filter to write the cookie headers, the filter seems to have no affect at all.
    /// </remarks>
    public sealed class UmbracoBackOfficeLogoutAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext context)
        {            
            if (context.Response == null) return;
            if (context.Response.IsSuccessStatusCode == false) return;
            context.Response.UmbracoLogoutWebApi();
        }
    }
}