using System;
using System.Threading.Tasks;

namespace Umbraco.Cms.Core.Routing
{
    /// <summary>
    /// Routes requests.
    /// </summary>
    public interface IPublishedRouter
    {
        /// <summary>
        /// Creates a published request.
        /// </summary>
        /// <param name="uri">The current request Uri.</param>
        /// <returns>A published request builder.</returns>
        Task<IPublishedRequestBuilder> CreateRequestAsync(Uri uri);

        /// <summary>
        /// Sends a <see cref="IPublishedRequestBuilder"/> through the routing pipeline and builds a result.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="options">The options.</param>
        /// <returns>The built <see cref="IPublishedRequest"/> instance.</returns>
        Task<IPublishedRequest> RouteRequestAsync(IPublishedRequestBuilder request, RouteRequestOptions options);

        /// <summary>
        /// Updates the request to "not found".
        /// </summary>
        /// <param name="request">The request.</param>
        /// <remarks>
        /// <returns>A new <see cref="IPublishedRequestBuilder"/> based on values from the original <see cref="IPublishedRequest"/></returns>
        /// <para>This method is invoked when the pipeline decides it cannot render
        /// the request, for whatever reason, and wants to force it to be re-routed
        /// and rendered as if no document were found (404).</para>
        /// <para>This occurs if there is no template found and route hijacking was not matched.
        /// In that case it's the same as if there was no content which means even if there was
        /// content matched we want to run the request through the last chance finders.</para>
        /// </remarks>
        Task<IPublishedRequestBuilder> UpdateRequestToNotFoundAsync(IPublishedRequest request);
    }
}
