using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
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

    // TODO (V19): Take the view path as a constructor parameter, as MaintenanceResult does, and drop the
    // service location in ExecuteResultAsync.

    /// <summary>
    ///     Initializes a new instance of the <see cref="PublishedContentNotFoundResult" /> class.
    /// </summary>
    public PublishedContentNotFoundResult(IUmbracoContext umbracoContext, string? message = null)
    {
        _umbracoContext = umbracoContext;
        _message = message;
    }

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

        // Resolved from the request rather than injected: this type is public and constructed with `new` by
        // callers outside this assembly, so taking the settings as a constructor parameter would be a binary
        // breaking change.
        GlobalSettings globalSettings = context.HttpContext.RequestServices
            .GetRequiredService<IOptionsMonitor<GlobalSettings>>().CurrentValue;

        var viewResult = new ViewResult { ViewName = globalSettings.NotFoundViewPath };
        context.HttpContext.Items.Add(
            "reason",
            string.Format(reason, WebUtility.HtmlEncode(_umbracoContext.OriginalRequestUrl.PathAndQuery)));
        context.HttpContext.Items.Add("message", _message);

        await viewResult.ExecuteResultAsync(context);
    }
}
