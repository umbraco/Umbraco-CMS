using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Web.Website.ActionResults;

/// <summary>
///     Redirects to the current URL rendering an Umbraco page including it's query strings
/// </summary>
/// <remarks>
///     This is useful if you need to redirect
///     to the current page but the current page is actually a rewritten URL normally done with something like
///     Server.Transfer. It is also handy if you want to persist the query strings.
/// </remarks>
public class RedirectToUmbracoUrlResult : IKeepTempDataResult
{
    private readonly IUmbracoContext _umbracoContext;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RedirectToUmbracoUrlResult" /> class.
    /// </summary>
    public RedirectToUmbracoUrlResult(IUmbracoContext umbracoContext) => _umbracoContext = umbracoContext;

    /// <inheritdoc />
    public Task ExecuteResultAsync(ActionContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var destinationUrl = _umbracoContext.OriginalRequestUrl.PathAndQuery;

        context.HttpContext.Response.Redirect(destinationUrl);

        return Task.CompletedTask;
    }
}
