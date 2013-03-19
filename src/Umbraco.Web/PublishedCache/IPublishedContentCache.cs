using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Umbraco.Core.Models;

namespace Umbraco.Web.PublishedCache
{
    internal interface IPublishedContentCache : IPublishedCache
    {
        // FIXME do we want that one?
        IPublishedContent GetByRoute(UmbracoContext umbracoContext, string route, bool? hideTopLevelNode = null);

        // FIXME do we want that one?
        IPublishedContent GetByUrlAlias(UmbracoContext umbracoContext, int rootNodeId, string alias);

        /// <summary>
        /// Gets a value indicating whether the cache contains published content.
        /// </summary>
        /// <param name="umbracoContext">The context.</param>
        /// <returns>A value indicating whether the cache contains published content.</returns>
        bool HasContent(UmbracoContext umbracoContext);
    }
}
