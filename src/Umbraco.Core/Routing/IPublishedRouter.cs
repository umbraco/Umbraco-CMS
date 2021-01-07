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

        // TODO: This shouldn't be required and should be handled differently during route building
        ///// <summary>
        ///// Updates the request to "not found".
        ///// </summary>
        ///// <param name="request">The request.</param>
        ///// <remarks>
        ///// <para>This method is invoked when the pipeline decides it cannot render
        ///// the request, for whatever reason, and wants to force it to be re-routed
        ///// and rendered as if no document were found (404).</para>
        ///// </remarks>
        //void UpdateRequestToNotFound(IPublishedRequest request);
    }
}
