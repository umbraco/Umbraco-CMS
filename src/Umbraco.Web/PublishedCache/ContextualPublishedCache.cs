using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core.Models;

namespace Umbraco.Web.PublishedCache
{
    /// <summary>
    /// Provides access to cached contents in a specified context.
    /// </summary>
    internal abstract class ContextualPublishedCache
    {
        protected readonly UmbracoContext UmbracoContext;
        private readonly IPublishedCache _cache;

        protected ContextualPublishedCache(UmbracoContext umbracoContext, IPublishedCache cache)
        {
            UmbracoContext = umbracoContext;
            _cache = cache;
        }

        /// <summary>
        /// Gets a content identified by its unique identifier.
        /// </summary>
        /// <param name="contentId">The content unique identifier.</param>
        /// <returns>The content, or null.</returns>
        public virtual IPublishedContent GetById(int contentId)
        {
            return _cache.GetById(UmbracoContext, contentId);
        }

        /// <summary>
        /// Gets contents at root.
        /// </summary>
        /// <returns>The contents.</returns>
        public virtual IEnumerable<IPublishedContent> GetAtRoot()
        {
            return _cache.GetAtRoot(UmbracoContext);
        }

        /// <summary>
        /// Gets a content resulting from an XPath query.
        /// </summary>
        /// <param name="xpath">The XPath query.</param>
        /// <param name="vars">Optional XPath variables.</param>
        /// <returns>The content, or null.</returns>
        public virtual IPublishedContent GetSingleByXPath(string xpath, Core.Xml.XPathVariable[] vars)
        {
            return _cache.GetSingleByXPath(UmbracoContext, xpath, vars);
        }

        /// <summary>
        /// Gets contents resulting from an XPath query.
        /// </summary>
        /// <param name="xpath">The XPath query.</param>
        /// <param name="vars">Optional XPath variables.</param>
        /// <returns>The contents.</returns>
        public virtual IEnumerable<IPublishedContent> GetByXPath(string xpath, Core.Xml.XPathVariable[] vars)
        {
            return _cache.GetByXPath(UmbracoContext, xpath, vars);
        }

        /// <summary>
        /// Gets a value indicating whether the underlying non-contextual cache contains published content.
        /// </summary>
        /// <returns>A value indicating whether the underlying non-contextual cache contains published content.</returns>
        public virtual bool HasContent()
        {
            return _cache.HasContent();
        }
    }
}
