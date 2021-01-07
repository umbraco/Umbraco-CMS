using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Core.Models;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Routes requests.
    /// </summary>
    public interface IPublishedRouter
    {
        // TODO: consider this and UmbracoRouteValueTransformer - move some code around?

        /// <summary>
        /// Creates a published request.
        /// </summary>
        /// <param name="uri">The current request Uri.</param>
        /// <returns>A published request builder.</returns>
        Task<IPublishedRequestBuilder> CreateRequestAsync(Uri uri);

        /// <summary>
        /// Prepares a request for rendering.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>A value indicating whether the request was successfully prepared and can be rendered.</returns>
        Task<IPublishedRequest> RouteRequestAsync(IPublishedRequestBuilder request);

        /// <summary>
        /// Tries to route a request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>A value indicating whether the request can be routed to a document.</returns>
        Task<bool> TryRouteRequestAsync(IPublishedRequestBuilder request);

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
        IPublishedRequestBuilder UpdateRequestToNotFound(IPublishedRequest request);
    }
}
