using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Routes requests.
    /// </summary>
    public interface IPublishedRouter
    {
        // TODO: consider this and RenderRouteHandler - move some code around?

        /// <summary>
        /// Creates a published request.
        /// </summary>
        /// <param name="umbracoContext">The current Umbraco context.</param>
        /// <param name="uri">The (optional) request Uri.</param>
        /// <returns>A published request.</returns>
        PublishedRequest CreateRequest(UmbracoContext umbracoContext, Uri uri = null);

        /// <summary>
        /// Prepares a request for rendering.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>A value indicating whether the request was successfully prepared and can be rendered.</returns>
        bool PrepareRequest(PublishedRequest request);

        /// <summary>
        /// Tries to route a request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>A value indicating whether the request can be routed to a document.</returns>
        bool TryRouteRequest(PublishedRequest request);

        /// <summary>
        /// Gets a template.
        /// </summary>
        /// <param name="alias">The template alias</param>
        /// <returns>The template.</returns>
        ITemplate GetTemplate(string alias);

        /// <summary>
        /// Updates the request to "not found".
        /// </summary>
        /// <param name="request">The request.</param>
        /// <remarks>
        /// <para>This method is invoked when the pipeline decides it cannot render
        /// the request, for whatever reason, and wants to force it to be re-routed
        /// and rendered as if no document were found (404).</para>
        /// </remarks>
        void UpdateRequestToNotFound(PublishedRequest request);
    }
}
