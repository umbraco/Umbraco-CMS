using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Routing
{
    /// <summary>
    /// Used for notifying when an Umbraco request is being built
    /// </summary>
    public class RoutingRequestNotification : INotification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoutingRequestNotification"/> class.
        /// </summary>
        public RoutingRequestNotification(IPublishedRequestBuilder requestBuilder) => RequestBuilder = requestBuilder;

        /// <summary>
        /// Gets the <see cref="IPublishedRequestBuilder"/>
        /// </summary>
        public IPublishedRequestBuilder RequestBuilder { get; }
    }
}
