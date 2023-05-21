using System.Net;

namespace Umbraco.Cms.Core.Routing;

public static class PublishedRequestExtensions
{
    /// <summary>
    ///     Gets the <see cref="UmbracoRouteResult" />
    /// </summary>
    public static UmbracoRouteResult GetRouteResult(this IPublishedRequest publishedRequest)
    {
        if (publishedRequest.IsRedirect())
        {
            return UmbracoRouteResult.Redirect;
        }

        if (!publishedRequest.HasPublishedContent())
        {
            return UmbracoRouteResult.NotFound;
        }

        return UmbracoRouteResult.Success;
    }

    /// <summary>
    ///     Gets a value indicating whether the request was successfully routed
    /// </summary>
    public static bool Success(this IPublishedRequest publishedRequest)
        => !publishedRequest.IsRedirect() && publishedRequest.HasPublishedContent();

    /// <summary>
    ///     Sets the response status to be 404 not found
    /// </summary>
    public static IPublishedRequestBuilder SetIs404(this IPublishedRequestBuilder publishedRequest)
    {
        publishedRequest.SetResponseStatus((int)HttpStatusCode.NotFound);
        return publishedRequest;
    }

    /// <summary>
    ///     Gets a value indicating whether the content request has a content.
    /// </summary>
    public static bool HasPublishedContent(this IPublishedRequestBuilder publishedRequest) =>
        publishedRequest.PublishedContent != null;

    /// <summary>
    ///     Gets a value indicating whether the content request has a content.
    /// </summary>
    public static bool HasPublishedContent(this IPublishedRequest publishedRequest) =>
        publishedRequest.PublishedContent != null;

    /// <summary>
    ///     Gets a value indicating whether the content request has a template.
    /// </summary>
    public static bool HasTemplate(this IPublishedRequestBuilder publishedRequest) => publishedRequest.Template != null;

    /// <summary>
    ///     Gets a value indicating whether the content request has a template.
    /// </summary>
    public static bool HasTemplate(this IPublishedRequest publishedRequest) => publishedRequest.Template != null;

    /// <summary>
    ///     Gets the alias of the template to use to display the requested content.
    /// </summary>
    public static string? GetTemplateAlias(this IPublishedRequest publishedRequest) => publishedRequest.Template?.Alias;

    /// <summary>
    ///     Gets a value indicating whether the requested content could not be found.
    /// </summary>
    public static bool Is404(this IPublishedRequest publishedRequest) =>
        publishedRequest.ResponseStatusCode == (int)HttpStatusCode.NotFound;

    /// <summary>
    ///     Gets a value indicating whether the content request triggers a redirect (permanent or not).
    /// </summary>
    public static bool IsRedirect(this IPublishedRequestBuilder publishedRequest) =>
        publishedRequest.ResponseStatusCode == (int)HttpStatusCode.Redirect ||
        publishedRequest.ResponseStatusCode == (int)HttpStatusCode.Moved;

    /// <summary>
    ///     Gets indicating whether the content request triggers a redirect (permanent or not).
    /// </summary>
    public static bool IsRedirect(this IPublishedRequest publishedRequest) =>
        publishedRequest.ResponseStatusCode == (int)HttpStatusCode.Redirect ||
        publishedRequest.ResponseStatusCode == (int)HttpStatusCode.Moved;

    /// <summary>
    ///     Gets a value indicating whether the redirect is permanent.
    /// </summary>
    public static bool IsRedirectPermanent(this IPublishedRequest publishedRequest) =>
        publishedRequest.ResponseStatusCode == (int)HttpStatusCode.Moved;

    /// <summary>
    ///     Gets a value indicating whether the content request has a domain.
    /// </summary>
    public static bool HasDomain(this IPublishedRequestBuilder publishedRequest) => publishedRequest.Domain != null;

    /// <summary>
    ///     Gets a value indicating whether the content request has a domain.
    /// </summary>
    public static bool HasDomain(this IPublishedRequest publishedRequest) => publishedRequest.Domain != null;
}
