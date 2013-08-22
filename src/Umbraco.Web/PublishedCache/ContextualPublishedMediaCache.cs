using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core.Models;

namespace Umbraco.Web.PublishedCache
{
    /// <summary>
    /// Provides access to cached medias in a specified context.
    /// </summary>
    public class ContextualPublishedMediaCache : ContextualPublishedCache<IPublishedMediaCache>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContextualPublishedMediaCache"/> class with a published media cache and a context.
        /// </summary>
        /// <param name="cache">A published media cache.</param>
        /// <param name="umbracoContext">A context.</param>
        internal ContextualPublishedMediaCache(IPublishedMediaCache cache, UmbracoContext umbracoContext)
            : base(umbracoContext, cache)
        { }
    }
}
