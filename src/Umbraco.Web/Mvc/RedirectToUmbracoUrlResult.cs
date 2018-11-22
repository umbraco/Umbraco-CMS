using System;
using System.Web.Mvc;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Redirects to the current URL rendering an Umbraco page including it's query strings
    /// </summary>
    /// <remarks>
    /// This is useful if you need to redirect 
    /// to the current page but the current page is actually a rewritten URL normally done with something like 
    /// Server.Transfer. It is also handy if you want to persist the query strings.
    /// </remarks>
    public class RedirectToUmbracoUrlResult : ActionResult
    {
        private readonly UmbracoContext _umbracoContext;

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>		
        /// <param name="umbracoContext"></param>
        public RedirectToUmbracoUrlResult(UmbracoContext umbracoContext)
        {
            _umbracoContext = umbracoContext;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            if (context.IsChildAction)
            {
                throw new InvalidOperationException("Cannot redirect from a Child Action");
            }

            var destinationUrl = _umbracoContext.OriginalRequestUrl.PathAndQuery;
            context.Controller.TempData.Keep();

            context.HttpContext.Response.Redirect(destinationUrl, endResponse: false);
        }
    }
}