using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Cms.Web.Website.ActionResults;

namespace Umbraco.Cms.Web.Website.Controllers;

/// <summary>
///     Provides a base class for front-end add-in controllers.
/// </summary>
[AutoValidateAntiforgeryToken]
public abstract class SurfaceController : PluginController
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SurfaceController" /> class.
    /// </summary>
    protected SurfaceController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger)
        => PublishedUrlProvider = publishedUrlProvider;

    protected IPublishedUrlProvider PublishedUrlProvider { get; }

    /// <summary>
    ///     Gets the current page.
    /// </summary>
    protected virtual IPublishedContent? CurrentPage
    {
        get
        {
            UmbracoRouteValues? umbracoRouteValues = HttpContext.Features.Get<UmbracoRouteValues>();
            if (umbracoRouteValues is null)
            {
                throw new InvalidOperationException(
                    $"No {nameof(UmbracoRouteValues)} feature was found in the HttpContext");
            }

            return umbracoRouteValues.PublishedRequest.PublishedContent;
        }
    }

    /// <summary>
    ///     Redirects to the Umbraco page with the given id
    /// </summary>
    protected RedirectToUmbracoPageResult RedirectToUmbracoPage(Guid contentKey)
        => new(contentKey, PublishedUrlProvider, UmbracoContextAccessor);

    /// <summary>
    ///     Redirects to the Umbraco page with the given id and passes provided querystring
    /// </summary>
    protected RedirectToUmbracoPageResult RedirectToUmbracoPage(Guid contentKey, QueryString queryString)
        => new(contentKey, queryString, PublishedUrlProvider, UmbracoContextAccessor);

    /// <summary>
    ///     Redirects to the Umbraco page with the given published content
    /// </summary>
    protected RedirectToUmbracoPageResult RedirectToUmbracoPage(IPublishedContent publishedContent)
        => new(publishedContent, PublishedUrlProvider, UmbracoContextAccessor);

    /// <summary>
    ///     Redirects to the Umbraco page with the given published content and passes provided querystring
    /// </summary>
    protected RedirectToUmbracoPageResult RedirectToUmbracoPage(
        IPublishedContent publishedContent,
        QueryString queryString)
        => new(publishedContent, queryString, PublishedUrlProvider, UmbracoContextAccessor);

    /// <summary>
    ///     Redirects to the currently rendered Umbraco page
    /// </summary>
    protected RedirectToUmbracoPageResult RedirectToCurrentUmbracoPage()
        => new(CurrentPage, PublishedUrlProvider, UmbracoContextAccessor);

    /// <summary>
    ///     Redirects to the currently rendered Umbraco page and passes provided querystring
    /// </summary>
    protected RedirectToUmbracoPageResult RedirectToCurrentUmbracoPage(QueryString queryString)
        => new(CurrentPage, queryString, PublishedUrlProvider, UmbracoContextAccessor);

    /// <summary>
    ///     Redirects to the currently rendered Umbraco URL
    /// </summary>
    /// <remarks>
    ///     This is useful if you need to redirect
    ///     to the current page but the current page is actually a rewritten URL normally done with something like
    ///     Server.Transfer.*
    /// </remarks>
    protected RedirectToUmbracoUrlResult RedirectToCurrentUmbracoUrl()
        => new(UmbracoContext);

    /// <summary>
    ///     Returns the currently rendered Umbraco page
    /// </summary>
    protected UmbracoPageResult CurrentUmbracoPage()
    {
        HttpContext.Features.Set(new ProxyViewDataFeature(ViewData, TempData));
        return new UmbracoPageResult(ProfilingLogger);
    }
}
