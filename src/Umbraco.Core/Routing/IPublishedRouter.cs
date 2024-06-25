using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Routes requests.
/// </summary>
public interface IPublishedRouter
{
    /// <summary>
    ///     Creates a published request.
    /// </summary>
    /// <param name="uri">The current request Uri.</param>
    /// <returns>A published request builder.</returns>
    Task<IPublishedRequestBuilder> CreateRequestAsync(Uri uri);

    /// <summary>
    ///     Sends a <see cref="IPublishedRequestBuilder" /> through the routing pipeline and builds a result.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="options">The options.</param>
    /// <returns>The built <see cref="IPublishedRequest" /> instance.</returns>
    Task<IPublishedRequest> RouteRequestAsync(IPublishedRequestBuilder request, RouteRequestOptions options);

    /// <summary>
    ///     Updates the request to use the specified <see cref="IPublishedContent" /> item, or NULL
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="publishedContent">The published content.</param>
    /// <remarks>
    ///     <returns>
    ///         A new <see cref="IPublishedRequest" /> based on values from the original <see cref="IPublishedRequest" />
    ///         and with the re-routed values based on the passed in <see cref="IPublishedContent" />
    ///     </returns>
    ///     <para>
    ///         This method is used for 2 cases:
    ///         - When the rendering content needs to change due to Public Access rules.
    ///         - When there is nothing to render due to circumstances such as no template files. In this case, NULL is used as the parameter.
    ///     </para>
    ///     <para>
    ///         This method is invoked when the pipeline decides it cannot render
    ///         the request, for whatever reason, and wants to force it to be re-routed
    ///         and rendered as if no document were found (404).
    ///         This occurs if there is no template found and route hijacking was not matched.
    ///         In that case it's the same as if there was no content which means even if there was
    ///         content matched we want to run the request through the last chance finders.
    ///     </para>
    /// </remarks>
    Task<IPublishedRequest> UpdateRequestAsync(IPublishedRequest request, IPublishedContent? publishedContent);

    /// <summary>
    /// Finds the site root (if any) matching the http request, and updates the PublishedRequest and VariationContext accordingly.
    /// <remarks>
    /// <para>
    /// This method is used for VirtualPage routing.
    /// </para>
    /// <para>
    /// In this case we do not want to run the entire routing pipeline since ContentFinders are not needed here.
    /// However, we do want to set the culture on VariationContext and PublishedRequest to the values specified by the domains.
    /// </para>
    /// </remarks>
    /// </summary>
    /// <param name="request">The request to update the culture on domain on</param>
    /// <returns>True if a domain was found otherwise false.</returns>
    bool RouteDomain(IPublishedRequestBuilder request) => false;

    /// <summary>
    /// Finds the site root (if any) matching the http request, and updates the VariationContext accordingly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is used for VirtualPage routing.
    /// </para>
    /// <para>
    /// This is required to set the culture on VariationContext to the values specified by the domains, before the FindContent method is called.
    /// In order to allow the FindContent implementer to correctly find content based off the culture. Before the PublishedRequest is built.
    /// </para>
    /// </remarks>
    /// <param name="uri">The URI to resolve the domain from.</param>
    /// <returns>True if a domain was found, otherwise false.</returns>
    bool UpdateVariationContext(Uri uri) => false;

}
