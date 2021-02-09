using System;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Routing
{
    /// <summary>
    /// Used for notifying when an Umbraco request is being created
    /// </summary>
    public class CreatingRequestNotification : INotification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreatingRequestNotification"/> class.
        /// </summary>
        public CreatingRequestNotification(Uri url) => Url = url;

        /// <summary>
        /// Gets or sets the URL for the request
        /// </summary>
        public Uri Url { get; set; }
    }
}
