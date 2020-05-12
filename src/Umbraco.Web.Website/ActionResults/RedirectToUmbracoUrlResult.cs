using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Web.Website.ActionResults
{
    /// <summary>
    /// Redirects to the current URL rendering an Umbraco page including it's query strings
    /// </summary>
    /// <remarks>
    /// This is useful if you need to redirect
    /// to the current page but the current page is actually a rewritten URL normally done with something like
    /// Server.Transfer. It is also handy if you want to persist the query strings.
    /// </remarks>
    public class RedirectToUmbracoUrlResult : IActionResult
    {
        private readonly IUmbracoContext _umbracoContext;

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>
        /// <param name="umbracoContext"></param>
        public RedirectToUmbracoUrlResult(IUmbracoContext umbracoContext)
        {
            _umbracoContext = umbracoContext;
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));

            var destinationUrl = _umbracoContext.OriginalRequestUrl.PathAndQuery;
            var tempDataDictionaryFactory = context.HttpContext.RequestServices.GetRequiredService<ITempDataDictionaryFactory>();
            var tempData = tempDataDictionaryFactory.GetTempData(context.HttpContext);
            tempData?.Keep();

            context.HttpContext.Response.Redirect(destinationUrl);

            return Task.CompletedTask;
        }
    }
}
