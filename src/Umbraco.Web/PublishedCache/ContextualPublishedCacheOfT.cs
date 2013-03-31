using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using Umbraco.Core.Models;
using Umbraco.Core.Xml;

namespace Umbraco.Web.PublishedCache
{
    /// <summary>
    /// Provides access to cached contents in a specified context.
    /// </summary>
    /// <typeparam name="T">The type of the underlying published cache.</typeparam>
    internal abstract class ContextualPublishedCache<T> : ContextualPublishedCache
        where T : IPublishedCache
    {
        private readonly T _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextualPublishedCache{T}"/> with a context and a published cache.
        /// </summary>
        /// <param name="umbracoContext">The context.</param>
        /// <param name="cache">The cache.</param>
        protected ContextualPublishedCache(UmbracoContext umbracoContext, T cache)
            : base(umbracoContext)
        {
            _cache = cache;
        }

        /// <summary>
        /// Gets the underlying published cache.
        /// </summary>
        public T InnerCache { get { return _cache; } }

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
        /// <remarks>
        /// <para>If <param name="vars" /> is <c>null</c>, or is empty, or contains only one single
        /// value which itself is <c>null</c>, then variables are ignored.</para>
        /// <para>The XPath expression should reference variables as <c>$var</c>.</para>
        /// </remarks>
        public override IPublishedContent GetSingleByXPath(string xpath, params XPathVariable[] vars)
        {
            return _cache.GetSingleByXPath(UmbracoContext, xpath, vars);
        }

        /// <summary>
        /// Gets contents resulting from an XPath query.
        /// </summary>
        /// <param name="xpath">The XPath query.</param>
        /// <param name="vars">Optional XPath variables.</param>
        /// <returns>The contents.</returns>
        /// <remarks>
        /// <para>If <param name="vars" /> is <c>null</c>, or is empty, or contains only one single
        /// value which itself is <c>null</c>, then variables are ignored.</para>
        /// <para>The XPath expression should reference variables as <c>$var</c>.</para>
        /// </remarks>
        public override IEnumerable<IPublishedContent> GetByXPath(string xpath, params XPathVariable[] vars)
        {
            return _cache.GetByXPath(UmbracoContext, xpath, vars);
        }

        /// <summary>
        /// Gets an XPath navigator that can be used to navigate contents.
        /// </summary>
        /// <returns>The XPath navigator.</returns>
        public override XPathNavigator GetXPathNavigator()
        {
            return _cache.GetXPathNavigator(UmbracoContext);
        }

        /// <summary>
        /// Gets a value indicating whether the underlying non-contextual cache contains published content.
        /// </summary>
        /// <returns>A value indicating whether the underlying non-contextual cache contains published content.</returns>
        public override bool HasContent()
        {
            return _cache.HasContent(UmbracoContext);
        }
    }
}
