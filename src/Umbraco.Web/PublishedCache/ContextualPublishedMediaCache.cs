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
    internal class ContextualPublishedMediaCache : ContextualPublishedCache
    {
        private readonly IPublishedMediaCache _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextualPublishedMediaCache"/> class with a published media cache and a context.
        /// </summary>
        /// <param name="cache">A published media cache.</param>
        /// <param name="umbracoContext">A context.</param>
        public ContextualPublishedMediaCache(IPublishedMediaCache cache, UmbracoContext umbracoContext)
            : base(umbracoContext)
        {
            _cache = cache;
        }

        /// <summary>
        /// Gets a content identified by its unique identifier.
        /// </summary>
        /// <param name="contentId">The content unique identifier.</param>
        /// <returns>The content, or null.</returns>
        public override IPublishedContent GetById(int contentId)
        {
            return _cache.GetById(UmbracoContext, contentId);
        }

        /// <summary>
        /// Gets contents at root.
        /// </summary>
        /// <returns>The contents.</returns>
        public override IEnumerable<IPublishedContent> GetAtRoot()
        {
            return _cache.GetAtRoot(UmbracoContext);
        }

        /// <summary>
        /// Gets a content resulting from an XPath query.
        /// </summary>
        /// <param name="xpath">The XPath query.</param>
        /// <param name="vars">Optional XPath variables.</param>
        /// <returns>The content, or null.</returns>
        public override IPublishedContent GetSingleByXPath(string xpath, Core.Xml.XPathVariable[] vars)
        {
            return _cache.GetSingleByXPath(UmbracoContext, xpath, vars);
        }

        /// <summary>
        /// Gets contents resulting from an XPath query.
        /// </summary>
        /// <param name="xpath">The XPath query.</param>
        /// <param name="vars">Optional XPath variables.</param>
        /// <returns>The contents.</returns>
        public override IEnumerable<IPublishedContent> GetByXPath(string xpath, Core.Xml.XPathVariable[] vars)
        {
            return _cache.GetByXPath(UmbracoContext, xpath, vars);
        }
    }
}
