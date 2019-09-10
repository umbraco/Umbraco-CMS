using System;
using System.IO;
using System.Web;
using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Configuration;

namespace Umbraco.Web
{
    /// <summary>
    /// Utility class used to check if the current request is for a front-end request
    /// </summary>
    /// <remarks>
    /// There are various checks to determine if this is a front-end request such as checking if the request is part of any reserved paths or existing MVC routes.
    /// </remarks>
    public sealed class RoutableDocumentLookup
    {
        public RoutableDocumentLookup(ILogger logger, IGlobalSettings globalSettings)
        {
            _logger = logger;
            _globalSettings = globalSettings;
            _combinedRouteCollection = new Lazy<RouteCollection>(CreateRouteCollection);
        }

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

            // handle directory-urls used for asmx
            // TODO: legacy - what's the point really?
            var asmxPos = lpath.IndexOf(".asmx/", StringComparison.OrdinalIgnoreCase);
            if (asmxPos >= 0)
            {
                // use uri.AbsolutePath, not path, 'cos path has been lowercased
                httpContext.RewritePath(uri.AbsolutePath.Substring(0, asmxPos + 5), // filePath
                    uri.AbsolutePath.Substring(asmxPos + 5), // pathInfo
                    uri.Query.TrimStart('?'));
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
            if (maybeDoc && _globalSettings.IsReservedPathOrUrl(lpath, httpContext, _combinedRouteCollection.Value))
                maybeDoc = false;

            //NOTE: No need to warn, plus if we do we should log the document, as this message doesn't really tell us anything :)
            //if (!maybeDoc)
            //{
            //    Logger.Warn<UmbracoModule>("Not a document");
            //}
            return maybeDoc;
        }

        /// <summary>
        /// This is used to be passed into the GlobalSettings.IsReservedPathOrUrl and will include some 'fake' routes
        /// used to determine if a path is reserved.
        /// </summary>
        /// <remarks>
        /// This is basically used to reserve paths dynamically
        /// </remarks>
        private readonly Lazy<RouteCollection> _combinedRouteCollection;
        private readonly ILogger _logger;
        private readonly IGlobalSettings _globalSettings;

        private RouteCollection CreateRouteCollection()
        {
            var routes = new RouteCollection();

            foreach (var route in RouteTable.Routes)
                routes.Add(route);

            foreach (var reservedPath in UmbracoModule.ReservedPaths)
            {
                try
                {
                    routes.Add("_umbreserved_" + reservedPath.ReplaceNonAlphanumericChars(""),
                        new Route(reservedPath.TrimStart('/'), new StopRoutingHandler()));
                }
                catch (Exception ex)
                {
                    _logger.Error<UmbracoModule>("Could not add reserved path route", ex);
                }
            }

            return routes;
        }
    }
}
