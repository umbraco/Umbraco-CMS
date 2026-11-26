using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Web.Common.ActionsResults;

/// <summary>
///     Returns the Umbraco not found result
/// </summary>
public class PublishedContentNotFoundResult : IActionResult
{
    private readonly string? _message;
    private readonly IUmbracoContext _umbracoContext;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PublishedContentNotFoundResult" /> class.
    /// </summary>
    /// <param name="umbracoContext">The Umbraco context for the current request.</param>
    /// <param name="notFoundViewPath">The path of the view rendered for the not-found response.</param>
    /// <param name="message">An optional message passed to the view.</param>
    public PublishedContentNotFoundResult(IUmbracoContext umbracoContext, string notFoundViewPath, string? message = null)
    {
        _umbracoContext = umbracoContext;
        ViewName = notFoundViewPath;
        _message = message;
    }

    /// <summary>
    ///     Gets the path of the view rendered for the not-found response.
    /// </summary>
    public string ViewName { get; }

    /// <inheritdoc />
    public async Task ExecuteResultAsync(ActionContext context)
    {
        HttpResponse response = context.HttpContext.Response;

        response.Clear();

        response.StatusCode = StatusCodes.Status404NotFound;

        IPublishedRequest? frequest = _umbracoContext.PublishedRequest;
        var reason = "Cannot render the page at URL '{0}'.";
        if (frequest?.HasPublishedContent() == false)
        {
            reason = "No umbraco document matches the URL '{0}'.";
        }
        else if (frequest?.HasTemplate() == false)
        {
            reason = "No template exists to render the document at URL '{0}'.";
        }

        var viewResult = new ViewResult { ViewName = ViewName };
        context.HttpContext.Items.Add(
            "reason",
            string.Format(reason, WebUtility.HtmlEncode(_umbracoContext.OriginalRequestUrl.PathAndQuery)));
        context.HttpContext.Items.Add("message", _message);

        await viewResult.ExecuteResultAsync(context);
    }
}
