using System.Linq;
using System.Xml.XPath;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Xml;
using Umbraco.Web.Models;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web
{
    /// <summary>
    /// Provides extension methods to ContextualPublishedCache.
    /// </summary>
    public static class ContextualPublishedCacheExtensions
    {
        /// <summary>
        /// Gets a dynamic content identified by its unique identifier.
        /// </summary>
        /// <param name="cache">The contextual cache.</param>
        /// <param name="contentId">The content unique identifier.</param>
        /// <returns>The dynamic content, or null.</returns>
        public static dynamic GetDynamicById(this ContextualPublishedContentCache cache, int contentId)
        {
            var content = cache.GetById(contentId);
            return content == null ? DynamicNull.Null : new DynamicPublishedContent(content).AsDynamic();
        }

        /// <summary>
        /// Gets a dynamic content resulting from an XPath query.
        /// </summary>
        /// <param name="cache">The contextual cache.</param>
        /// <param name="xpath">The XPath query.</param>
        /// <param name="vars">Optional XPath variables</param>
        /// <returns>The dynamic content, or null.</returns>
        public static dynamic GetDynamicSingleByXPath(this ContextualPublishedContentCache cache, string xpath, params XPathVariable[] vars)
        {
            var content = cache.GetSingleByXPath(xpath, vars);
            return content == null ? DynamicNull.Null : new DynamicPublishedContent(content).AsDynamic();
        }

        /// <summary>
        /// Gets a dynamic content resulting from an XPath query.
        /// </summary>
        /// <param name="cache">The contextual cache.</param>
        /// <param name="xpath">The XPath query.</param>
        /// <param name="vars">Optional XPath variables</param>
        /// <returns>The dynamic content, or null.</returns>
        public static dynamic GetDynamicSingleByXPath(this ContextualPublishedContentCache cache, XPathExpression xpath, params XPathVariable[] vars)
        {
            var content = cache.GetSingleByXPath(xpath, vars);
            return content == null ? DynamicNull.Null : new DynamicPublishedContent(content).AsDynamic();
        }

        /// <summary>
        /// Gets dynamic contents resulting from an XPath query.
        /// </summary>
        /// <param name="cache">The contextual cache.</param>
        /// <param name="xpath">The XPath query.</param>
        /// <param name="vars">Optional XPath variables</param>
        /// <returns>The dynamic contents.</returns>
        public static dynamic GetDynamicByXPath(this ContextualPublishedContentCache cache, string xpath, params XPathVariable[] vars)
        {
            var content = cache.GetByXPath(xpath, vars);
            return new DynamicPublishedContentList(content.Select(c => new DynamicPublishedContent(c)));
        }

        /// <summary>
        /// Gets dynamic contents resulting from an XPath query.
        /// </summary>
        /// <param name="cache">The contextual cache.</param>
        /// <param name="xpath">The XPath query.</param>
        /// <param name="vars">Optional XPath variables</param>
        /// <returns>The dynamic contents.</returns>
        public static dynamic GetDynamicByXPath(this ContextualPublishedContentCache cache, XPathExpression xpath, params XPathVariable[] vars)
        {
            var content = cache.GetByXPath(xpath, vars);
            return new DynamicPublishedContentList(content.Select(c => new DynamicPublishedContent(c)));
        }

        /// <summary>
        /// Gets dynamic contents at root.
        /// </summary>
        /// <param name="cache">The contextual cache.</param>
        /// <returns>The dynamic contents.</returns>
        public static dynamic GetDynamicAtRoot(this ContextualPublishedContentCache cache)
        {
            var content = cache.GetAtRoot();
            return new DynamicPublishedContentList(content.Select(c => new DynamicPublishedContent(c)));
        }
    }
}
