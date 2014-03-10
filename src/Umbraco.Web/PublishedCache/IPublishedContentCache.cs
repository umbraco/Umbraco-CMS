using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Umbraco.Core.Models;

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
        /// Creates a content fragment.
        /// </summary>
        /// <param name="contentTypeAlias">The content type alias.</param>
        /// <param name="dataValues">The content property raw values.</param>
        /// <param name="isPreviewing">A value indicating whether the fragment is previewing.</param>
        /// <param name="managed">A value indicating whether the fragment is managed by the cache.</param>
        /// <returns>The newly created content fragment.</returns>
        //
        // notes
        //
        // in XmlPublishedCache, IPublishedContent instances are not meant to survive longer
        // that a request or else we cannot guarantee that the converted property values will
        // be properly managed - because XmlPublishedProperty just stores the result of the
        // conversion locally.
        //
        // in DrippingPublishedCache, IPublishedContent instances are meant to survive for as
        // long as the content itself has not been modified, and the property respects the 
        // converter's indication ie whether the converted value should be cached at
        //   .Content - cache until the content changes
        //   .ContentCache - cache until any content changes
        //   .Request - cache for the current request
        //
        // a fragment can be either "detached" or "managed".
        //   detached: created from code, managed by code, converted property values are
        //     cached within the fragment itself for as long as the fragment lives
        //   managed: created from a property converter as part of a content, managed by
        //     the cache, converted property values can be cached...
        //
        //     XmlPublishedCache: same as content properties, store the result of the
        //       conversion locally, neither content nor fragments should survive longer
        //       than a request
        //     DrippingPublishedCache: depends
        //       .Content: cache within the fragment
        //       .ContentCache, .Request: cache within the cache
        //
        // in the latter case, use a fragment-owned guid as the cache key. because we
        // don't really have any other choice. this opens potential memory leaks: if the 
        // fragment is re-created on each request and has a property that caches its
        // converted value at .ContentCache level then we'll flood that cache with data
        // that's never removed (as long as no content is edited).
        //
        // so a requirement should be that any converter that creates fragment, should
        // be marked .Content -- and nothing else
        //
        IPublishedContent CreateFragment(string contentTypeAlias, IDictionary<string, object> dataValues,
            bool isPreviewing, bool managed);
    }
}
