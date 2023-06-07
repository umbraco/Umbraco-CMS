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
}
