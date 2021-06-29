using System;
using System.IO;
using System.Web;
using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Configuration;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.IO;
using System.Collections.Concurrent;
using Umbraco.Core.Collections;

namespace Umbraco.Web
{
    /// <summary>
    /// Utility class used to check if the current request is for a front-end request
    /// </summary>
    /// <remarks>
    /// There are various checks to determine if this is a front-end request such as checking if the request is part of any reserved paths or existing MVC routes.
    /// </remarks>
    public sealed class RoutableDocumentFilter
    {
        public RoutableDocumentFilter(IGlobalSettings globalSettings)
        {
            _globalSettings = globalSettings;
        }

        private static readonly ConcurrentDictionary<string, bool> RouteChecks = new ConcurrentDictionary<string, bool>();
        private readonly IGlobalSettings _globalSettings;
        private object _locker = new object();
        private bool _isInit = false;
        private int? _routeCount;
        private HashSet<string> _reservedList;

        /// <summary>
        /// Checks if the request is a document request (i.e. one that the module should handle)
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        public bool IsDocumentRequest(HttpContextBase httpContext, Uri uri)
        {
            var maybeDoc = true;
            var lpath = uri.AbsolutePath.ToLowerInvariant();

            // handle directory-URLs used for asmx
            // TODO: legacy - what's the point really?
            var asmxPos = lpath.IndexOf(".asmx/", StringComparison.OrdinalIgnoreCase);
            if (asmxPos >= 0)
            {
                // use uri.AbsolutePath, not path, 'cos path has been lowercased
                httpContext.RewritePath(uri.AbsolutePath.Substring(0, asmxPos + 5), // filePath
                    uri.AbsolutePath.Substring(asmxPos + 5), // pathInfo
                    uri.Query.TrimStart(Constants.CharArrays.QuestionMark));
                maybeDoc = false;
            }

            // a document request should be
            // /foo/bar/nil
            // /foo/bar/nil/
            // /foo/bar/nil.aspx
            // where /foo is not a reserved path

            // if the path contains an extension that is not .aspx
            // then it cannot be a document request
            var extension = Path.GetExtension(lpath);
            if (maybeDoc && extension.IsNullOrWhiteSpace() == false && extension != ".aspx")
                maybeDoc = false;

            // at that point, either we have no extension, or it is .aspx

            // if the path is reserved then it cannot be a document request
            if (maybeDoc && IsReservedPathOrUrl(lpath, httpContext, RouteTable.Routes))
                maybeDoc = false;

            //NOTE: No need to warn, plus if we do we should log the document, as this message doesn't really tell us anything :)
            //if (!maybeDoc)
            //{
            //    Logger.Warn<UmbracoModule>("Not a document");
            //}
            return maybeDoc;
        }

        /// <summary>
        /// Determines whether the specified URL is reserved or is inside a reserved path.
        /// </summary>
        /// <param name="url">The URL to check.</param>
        /// <returns>
        ///     <c>true</c> if the specified URL is reserved; otherwise, <c>false</c>.
        /// </returns>
        internal bool IsReservedPathOrUrl(string url)
        {
            LazyInitializer.EnsureInitialized(ref _reservedList, ref _isInit, ref _locker, () =>
            {
                // store references to strings to determine changes
                var reservedPathsCache = _globalSettings.ReservedPaths;
                var reservedUrlsCache = _globalSettings.ReservedUrls;

                // add URLs and paths to a new list
                var newReservedList = new HashSet<string>();
                foreach (var reservedUrlTrimmed in reservedUrlsCache
                    .Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim().ToLowerInvariant())
                    .Where(x => x.IsNullOrWhiteSpace() == false)
                    .Select(reservedUrl => IOHelper.ResolveUrl(reservedUrl).Trim().EnsureStartsWith("/"))
                    .Where(reservedUrlTrimmed => reservedUrlTrimmed.IsNullOrWhiteSpace() == false))
                {
                    newReservedList.Add(reservedUrlTrimmed);
                }

                foreach (var reservedPathTrimmed in NormalizePaths(reservedPathsCache.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries)))
                {
                    newReservedList.Add(reservedPathTrimmed);
                }

                foreach (var reservedPathTrimmed in NormalizePaths(ReservedPaths))
                {
                    newReservedList.Add(reservedPathTrimmed);
                }

                // use the new list from now on
                return newReservedList;
            });

            //The URL should be cleaned up before checking:
            // * If it doesn't contain an '.' in the path then we assume it is a path based URL, if that is the case we should add an trailing '/' because all of our reservedPaths use a trailing '/'
            // * We shouldn't be comparing the query at all
            var pathPart = url.Split(Constants.CharArrays.QuestionMark, StringSplitOptions.RemoveEmptyEntries)[0].ToLowerInvariant();
            if (pathPart.Contains(".") == false)
            {
                pathPart = pathPart.EnsureEndsWith('/');
            }

            // return true if URL starts with an element of the reserved list
            return _reservedList.Any(x => pathPart.InvariantStartsWith(x));
        }

        private IEnumerable<string> NormalizePaths(IEnumerable<string> paths)
        {
            return paths
                .Select(x => x.Trim().ToLowerInvariant())
                .Where(x => x.IsNullOrWhiteSpace() == false)
                .Select(reservedPath => IOHelper.ResolveUrl(reservedPath).Trim().EnsureStartsWith("/").EnsureEndsWith("/"))
                .Where(reservedPathTrimmed => reservedPathTrimmed.IsNullOrWhiteSpace() == false);
        }

        /// <summary>
        /// Determines whether the current request is reserved based on the route table and
        /// whether the specified URL is reserved or is inside a reserved path.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="httpContext"></param>
        /// <param name="routes">The route collection to lookup the request in</param>
        /// <returns></returns>
        internal bool IsReservedPathOrUrl(string url, HttpContextBase httpContext, RouteCollection routes)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));
            if (routes == null) throw new ArgumentNullException(nameof(routes));

            //This is some rudimentary code to check if the route table has changed at runtime, we're basically just keeping a count
            //of the routes. This isn't fail safe but there's no way to monitor changes to the route table. Else we need to create a hash
            //of all routes and then recompare but that will be annoying to do on each request and then we might as well just do the whole MVC
            //route on each request like we were doing before instead of caching the result of GetRouteData.
            var changed = false;
            using (routes.GetReadLock())
            {
                if (!_routeCount.HasValue || _routeCount.Value != routes.Count)
                {
                    //the counts are not set or have changed, need to reset
                    changed = true;
                }   
            }
            if (changed)
            {
                using (routes.GetWriteLock())
                {
                    _routeCount = routes.Count;

                    //try clearing each entry
                    foreach(var r in RouteChecks.Keys.ToList())
                        RouteChecks.TryRemove(r, out _);
                }
            }

            var absPath = httpContext?.Request?.Url.AbsolutePath;

            if (absPath.IsNullOrWhiteSpace())
                return false;

            //check if the current request matches a route, if so then it is reserved.
            var hasRoute = RouteChecks.GetOrAdd(absPath, x => routes.GetRouteData(httpContext) != null);
            if (hasRoute)
                return true;

            //continue with the standard ignore routine
            return IsReservedPathOrUrl(url);
        }

        /// <summary>
        /// This is used internally to track any registered callback paths for Identity providers. If the request path matches
        /// any of the registered paths, then the module will let the request keep executing
        /// </summary>
        internal static readonly ConcurrentHashSet<string> ReservedPaths = new ConcurrentHashSet<string>();
    }
}
