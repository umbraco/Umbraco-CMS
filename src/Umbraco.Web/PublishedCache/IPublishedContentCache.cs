using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Models;

namespace Umbraco.Web.PublishedCache
{
    public interface IPublishedContentCache : IPublishedCache
    {
        /// <summary>
        /// Gets content identified by a route.
        /// </summary>
        /// <param name="umbracoContext">The context.</param>
        /// <param name="preview">A value indicating whether to consider unpublished content.</param>
        /// <param name="route">The route</param>
        /// <param name="hideTopLevelNode">A value forcing the HideTopLevelNode setting.</param>
        /// <returns>The content, or null.</returns>
        /// <remarks>
        /// <para>A valid route is either a simple path eg <c>/foo/bar/nil</c> or a root node id and a path, eg <c>123/foo/bar/nil</c>.</para>
        /// <para>If <param name="hideTopLevelNode" /> is <c>null</c> then the settings value is used.</para>
        /// <para>The value of <paramref name="preview"/> overrides the context.</para>
        /// </remarks>
        IPublishedContent GetByRoute(UmbracoContext umbracoContext, bool preview, string route, bool? hideTopLevelNode = null);

        /// <summary>
        /// Gets the route for a content identified by its unique identifier.
        /// </summary>
        /// <param name="umbracoContext">The context.</param>
        /// <param name="preview">A value indicating whether to consider unpublished content.</param>
        /// <param name="contentId">The content unique identifier.</param>
        /// <returns>The route.</returns>
        /// <remarks>The value of <paramref name="preview"/> overrides the context.</remarks>
        string GetRouteById(UmbracoContext umbracoContext, bool preview, int contentId);

        /// <summary>
        /// Creates a detached property.
        /// </summary>
        /// <param name="propertyType">The published property type.</param>
        /// <param name="value">The value.</param>
        /// <param name="isPreviewing">A value indicating whether the property is created within a previewing context.</param>
        /// <returns>A detached property.</returns>
        /// <remarks>Implementations must check that propertyType.IsDetachedOrNested is true.</remarks>
        IPublishedProperty CreateDetachedProperty(PublishedPropertyType propertyType, object value, bool isPreviewing);
    }
}
