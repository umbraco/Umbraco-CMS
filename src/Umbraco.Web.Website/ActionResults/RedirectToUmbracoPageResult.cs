using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Website.ActionResults;

/// <summary>
///     Redirects to an Umbraco page by Id or Entity
/// </summary>
public class RedirectToUmbracoPageResult : IKeepTempDataResult
{
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly QueryString _queryString;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private IPublishedContent? _publishedContent;
    private string? _url;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RedirectToUmbracoPageResult" /> class.
    /// </summary>
    public RedirectToUmbracoPageResult(
        Guid key,
        IPublishedUrlProvider publishedUrlProvider,
        IUmbracoContextAccessor umbracoContextAccessor)
    {
        Key = key;
        _publishedUrlProvider = publishedUrlProvider;
        _umbracoContextAccessor = umbracoContextAccessor;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RedirectToUmbracoPageResult" /> class.
    /// </summary>
    public RedirectToUmbracoPageResult(
        Guid key,
        QueryString queryString,
        IPublishedUrlProvider publishedUrlProvider,
        IUmbracoContextAccessor umbracoContextAccessor)
    {
        Key = key;
        _queryString = queryString;
        _publishedUrlProvider = publishedUrlProvider;
        _umbracoContextAccessor = umbracoContextAccessor;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RedirectToUmbracoPageResult" /> class.
    /// </summary>
    public RedirectToUmbracoPageResult(
        IPublishedContent? publishedContent,
        IPublishedUrlProvider publishedUrlProvider,
        IUmbracoContextAccessor umbracoContextAccessor)
    {
        _publishedContent = publishedContent;
        Key = publishedContent?.Key ?? Guid.Empty;
        _publishedUrlProvider = publishedUrlProvider;
        _umbracoContextAccessor = umbracoContextAccessor;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RedirectToUmbracoPageResult" /> class.
    /// </summary>
    public RedirectToUmbracoPageResult(
        IPublishedContent? publishedContent,
        QueryString queryString,
        IPublishedUrlProvider publishedUrlProvider,
        IUmbracoContextAccessor umbracoContextAccessor)
    {
        _publishedContent = publishedContent;
        Key = publishedContent?.Key ?? Guid.Empty;
        _queryString = queryString;
        _publishedUrlProvider = publishedUrlProvider;
        _umbracoContextAccessor = umbracoContextAccessor;
    }

    public Guid Key { get; }

    private string Url
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_url))
            {
                return _url;
            }

            if (PublishedContent is null)
            {
                throw new InvalidOperationException($"Cannot redirect, no entity was found for key {Key}");
            }

            var result = _publishedUrlProvider.GetUrl(PublishedContent.Id);

            if (result == "#")
            {
                throw new InvalidOperationException(
                    $"Could not route to entity with key {Key}, the NiceUrlProvider could not generate a URL");
            }

            _url = result;

            return _url;
        }
    }

    private IPublishedContent? PublishedContent
    {
        get
        {
            if (!(_publishedContent is null))
            {
                return _publishedContent;
            }

            // need to get the URL for the page
            _publishedContent = _umbracoContextAccessor.GetRequiredUmbracoContext().Content?.GetById(Key);

            return _publishedContent;
        }
    }

    /// <inheritdoc />
    public Task ExecuteResultAsync(ActionContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        HttpContext httpContext = context.HttpContext;
        IUrlHelperFactory urlHelperFactory = httpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();
        IUrlHelper urlHelper = urlHelperFactory.GetUrlHelper(context);
        var destinationUrl = urlHelper.Content(Url);

        if (_queryString.HasValue)
        {
            destinationUrl += _queryString.ToUriComponent();
        }

        httpContext.Response.Redirect(destinationUrl);

        return Task.CompletedTask;
    }
}
