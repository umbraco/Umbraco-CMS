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

        protected ContextualPublishedCache(UmbracoContext umbracoContext)
        {
            UmbracoContext = umbracoContext;
        }

        /// <summary>
        /// Gets a content identified by its unique identifier.
        /// </summary>
        /// <param name="contentId">The content unique identifier.</param>
        /// <returns>The content, or null.</returns>
        public abstract IPublishedContent GetById(int contentId);

        /// <summary>
        /// Gets contents at root.
        /// </summary>
        /// <returns>The contents.</returns>
        public abstract IEnumerable<IPublishedContent> GetAtRoot();

        /// <summary>
        /// Gets a content resulting from an XPath query.
        /// </summary>
        /// <param name="xpath">The XPath query.</param>
        /// <param name="vars">Optional XPath variables.</param>
        /// <returns>The content, or null.</returns>
        public abstract IPublishedContent GetSingleByXPath(string xpath, Core.Xml.XPathVariable[] vars);

        /// <summary>
        /// Gets contents resulting from an XPath query.
        /// </summary>
        /// <param name="xpath">The XPath query.</param>
        /// <param name="vars">Optional XPath variables.</param>
        /// <returns>The contents.</returns>
        public abstract IEnumerable<IPublishedContent> GetByXPath(string xpath, Core.Xml.XPathVariable[] vars);
    }
}
