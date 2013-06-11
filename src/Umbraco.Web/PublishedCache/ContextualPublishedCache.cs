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
    public abstract class ContextualPublishedCache
    {
        protected readonly UmbracoContext UmbracoContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextualPublishedCache"/> with a context.
        /// </summary>
        /// <param name="umbracoContext">The context.</param>
        protected ContextualPublishedCache(UmbracoContext umbracoContext)
        {
            UmbracoContext = umbracoContext;
        }

        /// <summary>
        /// Gets a content identified by its unique identifier.
        /// </summary>
        /// <param name="contentId">The content unique identifier.</param>
        /// <returns>The content, or null.</returns>
        /// <remarks>Considers published or unpublished content depending on context.</remarks>
        public IPublishedContent GetById(int contentId)
        {
            return GetById(UmbracoContext.InPreviewMode, contentId);
        }

        /// <summary>
        /// Gets a content identified by its unique identifier.
        /// </summary>
        /// <param name="preview">A value indicating whether to consider unpublished content.</param>
        /// <param name="contentId">The content unique identifier.</param>
        /// <returns>The content, or null.</returns>
        public abstract IPublishedContent GetById(bool preview, int contentId);

        /// <summary>
        /// Gets content at root.
        /// </summary>
        /// <returns>The contents.</returns>
        /// <remarks>Considers published or unpublished content depending on context.</remarks>
        public IEnumerable<IPublishedContent> GetAtRoot()
        {
            return GetAtRoot(UmbracoContext.InPreviewMode);
        }

        /// <summary>
        /// Gets contents at root.
        /// </summary>
        /// <param name="preview">A value indicating whether to consider unpublished content.</param>
        /// <returns>The contents.</returns>
        public abstract IEnumerable<IPublishedContent> GetAtRoot(bool preview);

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
        /// <para>Considers published or unpublished content depending on context.</para>
        /// </remarks>
        public IPublishedContent GetSingleByXPath(string xpath, params XPathVariable[] vars)
        {
            return GetSingleByXPath(UmbracoContext.InPreviewMode, xpath, vars);
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
        /// <para>Considers published or unpublished content depending on context.</para>
        /// </remarks>
        public IPublishedContent GetSingleByXPath(XPathExpression xpath, params XPathVariable[] vars)
        {
            return GetSingleByXPath(UmbracoContext.InPreviewMode, xpath, vars);
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
        public abstract IPublishedContent GetSingleByXPath(bool preview, string xpath, params XPathVariable[] vars);

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
        public abstract IPublishedContent GetSingleByXPath(bool preview, XPathExpression xpath, params XPathVariable[] vars);

        /// <summary>
        /// Gets content resulting from an XPath query.
        /// </summary>
        /// <param name="xpath">The XPath query.</param>
        /// <param name="vars">Optional XPath variables.</param>
        /// <returns>The contents.</returns>
        /// <remarks>
        /// <para>If <param name="vars" /> is <c>null</c>, or is empty, or contains only one single
        /// value which itself is <c>null</c>, then variables are ignored.</para>
        /// <para>The XPath expression should reference variables as <c>$var</c>.</para>
        /// <para>Considers published or unpublished content depending on context.</para>
        /// </remarks>
        public IEnumerable<IPublishedContent> GetByXPath(string xpath, params XPathVariable[] vars)
        {
            return GetByXPath(UmbracoContext.InPreviewMode, xpath, vars);
        }

        /// <summary>
        /// Gets content resulting from an XPath query.
        /// </summary>
        /// <param name="xpath">The XPath query.</param>
        /// <param name="vars">Optional XPath variables.</param>
        /// <returns>The contents.</returns>
        /// <remarks>
        /// <para>If <param name="vars" /> is <c>null</c>, or is empty, or contains only one single
        /// value which itself is <c>null</c>, then variables are ignored.</para>
        /// <para>The XPath expression should reference variables as <c>$var</c>.</para>
        /// <para>Considers published or unpublished content depending on context.</para>
        /// </remarks>
        public IEnumerable<IPublishedContent> GetByXPath(XPathExpression xpath, params XPathVariable[] vars)
        {
            return GetByXPath(UmbracoContext.InPreviewMode, xpath, vars);
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
        public abstract IEnumerable<IPublishedContent> GetByXPath(bool preview, string xpath, params XPathVariable[] vars);

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
        public abstract IEnumerable<IPublishedContent> GetByXPath(bool preview, XPathExpression xpath, params XPathVariable[] vars);

        /// <summary>
        /// Gets an XPath navigator that can be used to navigate content.
        /// </summary>
        /// <returns>The XPath navigator.</returns>
        /// <remarks>Considers published or unpublished content depending on context.</remarks>
        public XPathNavigator GetXPathNavigator()
        {
            return GetXPathNavigator(UmbracoContext.InPreviewMode);
        }

        /// <summary>
        /// Gets an XPath navigator that can be used to navigate content.
        /// </summary>
        /// <param name="preview">A value indicating whether to consider unpublished content.</param>
        /// <returns>The XPath navigator.</returns>
        public abstract XPathNavigator GetXPathNavigator(bool preview);

        /// <summary>
        /// Gets a value indicating whether <c>GetXPathNavigator</c> returns an <c>XPathNavigator</c>
        /// and that navigator is a <c>NavigableNavigator</c>.
        /// </summary>
        public abstract bool XPathNavigatorIsNavigable { get; }

        /// <summary>
        /// Gets a value indicating whether the underlying non-contextual cache contains content.
        /// </summary>
        /// <returns>A value indicating whether the underlying non-contextual cache contains content.</returns>
        /// <remarks>Considers published or unpublished content depending on context.</remarks>
        public bool HasContent()
        {
            return HasContent(UmbracoContext.InPreviewMode);
        }

        /// <summary>
        /// Gets a value indicating whether the underlying non-contextual cache contains content.
        /// </summary>
        /// <param name="preview">A value indicating whether to consider unpublished content.</param>
        /// <returns>A value indicating whether the underlying non-contextual cache contains content.</returns>
        public abstract bool HasContent(bool preview);
    }
}
