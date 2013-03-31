using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Web.PublishedCache
{
    /// <summary>
    /// Provides caches (content and media).
    /// </summary>
    /// <remarks>Groups caches that _may_ be related.</remarks>
    interface IPublishedCaches
    {
        /// <summary>
        /// Gets the content cache.
        /// </summary>
        IPublishedContentCache ContentCache { get; }

        /// <summary>
        /// Gets the media cache.
        /// </summary>
        IPublishedMediaCache MediaCache { get; }
    }
}
