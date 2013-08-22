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
        /// Creates a contextual content cache for a specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A new contextual content cache for the specified context.</returns>
        ContextualPublishedContentCache CreateContextualContentCache(UmbracoContext context);

        /// <summary>
        /// Creates a contextual media cache for a specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A new contextual media cache for the specified context.</returns>
        ContextualPublishedMediaCache CreateContextualMediaCache(UmbracoContext context);
    }
}
