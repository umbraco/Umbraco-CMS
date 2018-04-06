using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Umbraco.Core.IO;

namespace Umbraco.Core.Configuration
{
    public static class GlobalSettingsExtensions
    {
        /// <summary>
        /// Used in unit testing to reset all config items, this is automatically called by GlobalSettings.Reset()
        /// </summary>
        internal static void Reset()
        {
            _reservedUrlsCache = null;
            _mvcArea = null;
        }

        private static readonly object Locker = new object();
        //make this volatile so that we can ensure thread safety with a double check lock
        private static volatile string _reservedUrlsCache;
        private static string _reservedPathsCache;
        private static HashSet<string> _reservedList = new HashSet<string>();
        private static string _mvcArea;

        /// <summary>
        /// This returns the string of the MVC Area route.
        /// </summary>
        /// <remarks>
        /// This will return the MVC area that we will route all custom routes through like surface controllers, etc...
        /// We will use the 'Path' (default ~/umbraco) to create it but since it cannot contain '/' and people may specify a path of ~/asdf/asdf/admin
        /// we will convert the '/' to '-' and use that as the path. its a bit lame but will work.
        ///
        /// We also make sure that the virtual directory (SystemDirectories.Root) is stripped off first, otherwise we'd end up with something
        /// like "MyVirtualDirectory-Umbraco" instead of just "Umbraco".
        /// </remarks>
        public static string GetUmbracoMvcArea(this IGlobalSettings globalSettings)
        {
            if (_mvcArea != null) return _mvcArea;

            if (globalSettings.Path.IsNullOrWhiteSpace())
            {
                throw new InvalidOperationException("Cannot create an MVC Area path without the umbracoPath specified");
            }

            var path = globalSettings.Path;
            if (path.StartsWith(SystemDirectories.Root)) // beware of TrimStart, see U4-2518
                path = path.Substring(SystemDirectories.Root.Length);
            _mvcArea = path.TrimStart('~').TrimStart('/').Replace('/', '-').Trim().ToLower();
            return _mvcArea;
        }

        /// <summary>
        /// Determines whether the specified URL is reserved or is inside a reserved path.
        /// </summary>
        /// <param name="globalSettings"></param>
        /// <param name="url">The URL to check.</param>
        /// <returns>
        ///     <c>true</c> if the specified URL is reserved; otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsReservedPathOrUrl(this IGlobalSettings globalSettings, string url)
        {
            if (_reservedUrlsCache == null)
            {
                lock (Locker)
                {
                    if (_reservedUrlsCache == null)
                    {
                        // store references to strings to determine changes
                        _reservedPathsCache = globalSettings.ReservedPaths;
                        _reservedUrlsCache = globalSettings.ReservedUrls;

                        // add URLs and paths to a new list
                        var newReservedList = new HashSet<string>();
                        foreach (var reservedUrlTrimmed in _reservedUrlsCache
                            .Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Trim().ToLowerInvariant())
                            .Where(x => x.IsNullOrWhiteSpace() == false)
                            .Select(reservedUrl => IOHelper.ResolveUrl(reservedUrl).Trim().EnsureStartsWith("/"))
                            .Where(reservedUrlTrimmed => reservedUrlTrimmed.IsNullOrWhiteSpace() == false))
                        {
                            newReservedList.Add(reservedUrlTrimmed);
                        }

                        foreach (var reservedPathTrimmed in _reservedPathsCache
                            .Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Trim().ToLowerInvariant())
                            .Where(x => x.IsNullOrWhiteSpace() == false)
                            .Select(reservedPath => IOHelper.ResolveUrl(reservedPath).Trim().EnsureStartsWith("/").EnsureEndsWith("/"))
                            .Where(reservedPathTrimmed => reservedPathTrimmed.IsNullOrWhiteSpace() == false))
                        {
                            newReservedList.Add(reservedPathTrimmed);
                        }

                        // use the new list from now on
                        _reservedList = newReservedList;
                    }
                }
            }

            //The url should be cleaned up before checking:
            // * If it doesn't contain an '.' in the path then we assume it is a path based URL, if that is the case we should add an trailing '/' because all of our reservedPaths use a trailing '/'
            // * We shouldn't be comparing the query at all
            var pathPart = url.Split(new[] {'?'}, StringSplitOptions.RemoveEmptyEntries)[0].ToLowerInvariant();
            if (pathPart.Contains(".") == false)
            {
                pathPart = pathPart.EnsureEndsWith('/');
            }

            // return true if url starts with an element of the reserved list
            return _reservedList.Any(x => pathPart.InvariantStartsWith(x));
        }

        /// <summary>
        /// Determines whether the current request is reserved based on the route table and
        /// whether the specified URL is reserved or is inside a reserved path.
        /// </summary>
        /// <param name="globalSettings"></param>
        /// <param name="url"></param>
        /// <param name="httpContext"></param>
        /// <param name="routes">The route collection to lookup the request in</param>
        /// <returns></returns>
        internal static bool IsReservedPathOrUrl(this IGlobalSettings globalSettings, string url, HttpContextBase httpContext, RouteCollection routes)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));
            if (routes == null) throw new ArgumentNullException(nameof(routes));

            //check if the current request matches a route, if so then it is reserved.
            var route = routes.GetRouteData(httpContext);
            if (route != null)
                return true;

            //continue with the standard ignore routine
            return globalSettings.IsReservedPathOrUrl(url);
        }


    }
}
