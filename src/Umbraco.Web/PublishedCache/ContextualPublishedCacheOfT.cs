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
    /// <remarks>The type <typeparamref name="T"/> differenciates between the content cache and the media cache,
    /// ie it will be either IPublishedContentCache or IPublishedMediaCache.</remarks>
    public abstract class ContextualPublishedCache<T> : ContextualPublishedCache
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
        /// <param name="preview">A value indicating whether to consider unpublished content.</param>
        /// <param name="contentId">The content unique identifier.</param>
        /// <returns>The content, or null.</returns>
        public override IPublishedContent GetById(bool preview, int contentId)
        {
            return _cache.GetById(UmbracoContext, preview, contentId);
        }

        /// <summary>
        /// Gets content at root.
        /// </summary>
        /// <param name="preview">A value indicating whether to consider unpublished content.</param>
        /// <returns>The contents.</returns>
        public override IEnumerable<IPublishedContent> GetAtRoot(bool preview)
        {
            return _cache.GetAtRoot(UmbracoContext, preview);
        }

        /// <summary>
        /// Gets a content resulting from an XPath query.
        /// </summary>
        /// <param name="preview">A value indicating whether to consider unpublished content.</param>
        /// <param name="xpath">The XPath query.</param>
        /// <param name="vars">Optional XPath variables.</param>
        /// <returns>The content, or null.</returns>
        /// <remarks>
        /// <para>If <param name="vars" /> is <c>null</c>, or is empty, or contains only one single
        /// value which itself is <c>null</c>, then variables are ignored.</para>
        /// <para>The XPath expression should reference variables as <c>$var</c>.</para>
        /// </remarks>
        public override IPublishedContent GetSingleByXPath(bool preview, string xpath, params XPathVariable[] vars)
        {
            return _cache.GetSingleByXPath(UmbracoContext, preview, xpath, vars);
        }

        /// <summary>
        /// Gets a content resulting from an XPath query.
        /// </summary>
        /// <param name="preview">A value indicating whether to consider unpublished content.</param>
        /// <param name="xpath">The XPath query.</param>
        /// <param name="vars">Optional XPath variables.</param>
        /// <returns>The content, or null.</returns>
        /// <remarks>
        /// <para>If <param name="vars" /> is <c>null</c>, or is empty, or contains only one single
        /// value which itself is <c>null</c>, then variables are ignored.</para>
        /// <para>The XPath expression should reference variables as <c>$var</c>.</para>
        /// </remarks>
        public override IPublishedContent GetSingleByXPath(bool preview, XPathExpression xpath, params XPathVariable[] vars)
        {
            return _cache.GetSingleByXPath(UmbracoContext, preview, xpath, vars);
        }

        /// <summary>
        /// Gets content resulting from an XPath query.
        /// </summary>
        /// <param name="preview">A value indicating whether to consider unpublished content.</param>
        /// <param name="xpath">The XPath query.</param>
        /// <param name="vars">Optional XPath variables.</param>
        /// <returns>The contents.</returns>
        /// <remarks>
        /// <para>If <param name="vars" /> is <c>null</c>, or is empty, or contains only one single
        /// value which itself is <c>null</c>, then variables are ignored.</para>
        /// <para>The XPath expression should reference variables as <c>$var</c>.</para>
        /// </remarks>
        public override IEnumerable<IPublishedContent> GetByXPath(bool preview, string xpath, params XPathVariable[] vars)
        {
            return _cache.GetByXPath(UmbracoContext, preview, xpath, vars);
        }

        /// <summary>
        /// Gets content resulting from an XPath query.
        /// </summary>
        /// <param name="preview">A value indicating whether to consider unpublished content.</param>
        /// <param name="xpath">The XPath query.</param>
        /// <param name="vars">Optional XPath variables.</param>
        /// <returns>The contents.</returns>
        /// <remarks>
        /// <para>If <param name="vars" /> is <c>null</c>, or is empty, or contains only one single
        /// value which itself is <c>null</c>, then variables are ignored.</para>
        /// <para>The XPath expression should reference variables as <c>$var</c>.</para>
        /// </remarks>
        public override IEnumerable<IPublishedContent> GetByXPath(bool preview, XPathExpression xpath, params XPathVariable[] vars)
        {
            return _cache.GetByXPath(UmbracoContext, preview, xpath, vars);
        }

        /// <summary>
        /// Gets an XPath navigator that can be used to navigate content.
        /// </summary>
        /// <param name="preview">A value indicating whether to consider unpublished content.</param>
        /// <returns>The XPath navigator.</returns>
        public override XPathNavigator GetXPathNavigator(bool preview)
        {
            return _cache.GetXPathNavigator(UmbracoContext, preview);
        }

         /// <summary>
        /// Gets a value indicating whether <c>GetXPathNavigator</c> returns an <c>XPathNavigator</c>
        /// and that navigator is a <c>NavigableNavigator</c>.
        /// </summary>
        public override bool XPathNavigatorIsNavigable { get { return _cache.XPathNavigatorIsNavigable; } }

        /// <summary>
        /// Gets a value indicating whether the underlying non-contextual cache contains content.
        /// </summary>
        /// <param name="preview">A value indicating whether to consider unpublished content.</param>
        /// <returns>A value indicating whether the underlying non-contextual cache contains content.</returns>
        public override bool HasContent(bool preview)
        {
            return _cache.HasContent(UmbracoContext, preview);
        }
    }
}
