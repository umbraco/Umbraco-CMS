using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace Umbraco.Core
{
    /// <summary>
    /// Provides extension methods to <see cref="Uri"/>.
    /// </summary>
    public static class UriExtensions
    {
        ///  <summary>
        ///  Checks if the current uri is a back office request
        ///  </summary>
        ///  <param name="url"></param>
        ///  <param name="applicationPath">
        ///  The current application path or VirtualPath
        ///  </param>
        /// <param name="globalSettings"></param>
        /// <returns></returns>
        ///  <remarks>
        ///  There are some special routes we need to check to properly determine this:
        ///
        ///      If any route has an extension in the path like .aspx = back office
        ///
        ///      These are def back office:
        ///          /Umbraco/BackOffice     = back office
        ///          /Umbraco/Preview        = back office
        ///      If it's not any of the above, and there's no extension then we cannot determine if it's back office or front-end
        ///      so we can only assume that it is not back office. This will occur if people use an UmbracoApiController for the backoffice
        ///      but do not inherit from UmbracoAuthorizedApiController and do not use [IsBackOffice] attribute.
        ///
        ///      These are def front-end:
        ///          /Umbraco/Surface        = front-end
        ///          /Umbraco/Api            = front-end
        ///      But if we've got this far we'll just have to assume it's front-end anyways.
        ///
        ///  </remarks>
        internal static bool IsBackOfficeRequest(this Uri url, string applicationPath, IGlobalSettings globalSettings)
        {
            applicationPath = applicationPath ?? string.Empty;

            var fullUrlPath = url.AbsolutePath.TrimStart(Constants.CharArrays.ForwardSlash);
            var appPath = applicationPath.TrimStart(Constants.CharArrays.ForwardSlash);
            var urlPath = fullUrlPath.TrimStart(appPath).EnsureStartsWith('/');

            //check if this is in the umbraco back office
            var isUmbracoPath = urlPath.InvariantStartsWith(globalSettings.Path.EnsureStartsWith('/').TrimStart(appPath.EnsureStartsWith('/')).EnsureStartsWith('/'));
            //if not, then def not back office
            if (isUmbracoPath == false) return false;

            //if its the normal /umbraco path
            if (urlPath.InvariantEquals("/" + globalSettings.GetUmbracoMvcArea())
                || urlPath.InvariantEquals("/" + globalSettings.GetUmbracoMvcArea() + "/"))
            {
                return true;
            }

            //check for a file extension
            var extension = Path.GetExtension(url.LocalPath);
            //has an extension, def back office
            if (extension.IsNullOrWhiteSpace() == false) return true;
            //check for special case asp.net calls like:
            //  /umbraco/webservices/legacyAjaxCalls.asmx/js which will return a null file extension but are still considered requests with an extension
            if (urlPath.InvariantContains(".asmx/")
                || urlPath.InvariantContains(".aspx/")
                || urlPath.InvariantContains(".ashx/")
                || urlPath.InvariantContains(".axd/")
                || urlPath.InvariantContains(".svc/"))
            {
                return true;
            }

            //check for special back office paths
            if (urlPath.InvariantStartsWith("/" + globalSettings.GetUmbracoMvcArea() + "/BackOffice/")
                || urlPath.InvariantStartsWith("/" + globalSettings.GetUmbracoMvcArea() + "/Preview/"))
            {
                return true;
            }

            //check for special front-end paths
            if (urlPath.InvariantStartsWith("/" + globalSettings.GetUmbracoMvcArea() + "/Surface/")
                || urlPath.InvariantStartsWith("/" + globalSettings.GetUmbracoMvcArea() + "/Api/"))
            {
                return false;
            }

            //if its none of the above, we will have to try to detect if it's a PluginController route, we can detect this by
            // checking how many parts the route has, for example, all PluginController routes will be routed like
            // Umbraco/MYPLUGINAREA/MYCONTROLLERNAME/{action}/{id}
            // so if the path contains at a minimum 3 parts: Umbraco + MYPLUGINAREA + MYCONTROLLERNAME then we will have to assume it is a
            // plugin controller for the front-end.
            if (urlPath.Split(Constants.CharArrays.ForwardSlash, StringSplitOptions.RemoveEmptyEntries).Length >= 3)
            {
                return false;
            }

            //if its anything else we can assume it's back office
            return true;
        }

        /// <summary>
        /// Checks if the current uri is an install request
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        internal static bool IsInstallerRequest(this Uri url)
        {
            var authority = url.GetLeftPart(UriPartial.Authority);
            var afterAuthority = url.GetLeftPart(UriPartial.Query)
                                    .TrimStart(authority)
                                    .TrimStart("/");

            //check if this is in the umbraco back office
            return afterAuthority.InvariantStartsWith(IOHelper.ResolveUrl("~/install").TrimStart("/"));
        }

        /// <summary>
        /// Checks if the uri is a request for the default back office page
        /// </summary>
        /// <param name="url"></param>
        /// <param name="globalSettings"></param>
        /// <returns></returns>
        internal static bool IsDefaultBackOfficeRequest(this Uri url, IGlobalSettings globalSettings)
        {
            if (url.AbsolutePath.InvariantEquals(globalSettings.Path.TrimEnd("/"))
                || url.AbsolutePath.InvariantEquals(globalSettings.Path.EnsureEndsWith('/'))
                || url.AbsolutePath.InvariantEquals(globalSettings.Path.EnsureEndsWith('/') + "Default")
                || url.AbsolutePath.InvariantEquals(globalSettings.Path.EnsureEndsWith('/') + "Default/"))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Non Client Side request Extensions <see cref="IsClientSideRequest"/>
        /// </summary>
        internal readonly static HashSet<string> NonClientSideRequestExtensions = new (5, StringComparer.InvariantCultureIgnoreCase)
        {
            ".aspx", ".ashx", ".asmx", ".axd", ".svc"
        };

        /// <summary>
        /// This is a performance tweak to check if this not an ASP.Net server file
        /// .Net will pass these requests through to the module when in integrated mode.
        /// We want to ignore all of these requests immediately.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        internal static bool IsClientSideRequest(this Uri url)
        {
            try
            {
                var ext = Path.GetExtension(url.LocalPath);
                if (ext.IsNullOrWhiteSpace()) return false;
                return !NonClientSideRequestExtensions.Contains(ext);
            }
            catch (ArgumentException)
            {
                Current.Logger.Debug<string>(typeof(UriExtensions), "Failed to determine if request was client side (invalid chars in path \"{Path}\"?)", url.LocalPath);
                return false;
            }
        }

        /// <summary>
        /// Rewrites the path of uri.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <param name="path">The new path, which must begin with a slash.</param>
        /// <returns>The rewritten uri.</returns>
        /// <remarks>Everything else remains unchanged, except for the fragment which is removed.</remarks>
        public static Uri Rewrite(this Uri uri, string path)
        {
            if (path.StartsWith("/") == false)
                throw new ArgumentException("Path must start with a slash.", "path");

            return uri.IsAbsoluteUri
                ? new Uri(uri.GetLeftPart(UriPartial.Authority) + path + uri.Query)
                : new Uri(path + uri.GetSafeQuery(), UriKind.Relative);
        }

        /// <summary>
        /// Rewrites the path and query of a uri.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <param name="path">The new path, which must begin with a slash.</param>
        /// <param name="query">The new query, which must be empty or begin with a question mark.</param>
        /// <returns>The rewritten uri.</returns>
        /// <remarks>Everything else remains unchanged, except for the fragment which is removed.</remarks>
        public static Uri Rewrite(this Uri uri, string path, string query)
        {
            if (path.StartsWith("/") == false)
                throw new ArgumentException("Path must start with a slash.", "path");
            if (query.Length > 0 && query.StartsWith("?") == false)
                throw new ArgumentException("Query must start with a question mark.", "query");
            if (query == "?")
                query = "";

            return uri.IsAbsoluteUri
                ? new Uri(uri.GetLeftPart(UriPartial.Authority) + path + query)
                : new Uri(path + query, UriKind.Relative);
        }

        /// <summary>
        /// Gets the absolute path of the uri, even if the uri is relative.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <returns>The absolute path of the uri.</returns>
        /// <remarks>Default uri.AbsolutePath does not support relative uris.</remarks>
        public static string GetSafeAbsolutePath(this Uri uri)
        {
            if (uri.IsAbsoluteUri)
                return uri.AbsolutePath;

            // cannot get .AbsolutePath on relative uri (InvalidOperation)
            var s = uri.OriginalString;
            var posq = s.IndexOf("?", StringComparison.Ordinal);
            var posf = s.IndexOf("#", StringComparison.Ordinal);
            var pos = posq > 0 ? posq : (posf > 0 ? posf : 0);
            var path = pos > 0 ? s.Substring(0, pos) : s;
            return path;
        }

        /// <summary>
        /// Gets the decoded, absolute path of the uri.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <returns>The absolute path of the uri.</returns>
        /// <remarks>Only for absolute uris.</remarks>
        public static string GetAbsolutePathDecoded(this Uri uri)
        {
            return System.Web.HttpUtility.UrlDecode(uri.AbsolutePath);
        }

        /// <summary>
        /// Gets the decoded, absolute path of the uri, even if the uri is relative.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <returns>The absolute path of the uri.</returns>
        /// <remarks>Default uri.AbsolutePath does not support relative uris.</remarks>
        public static string GetSafeAbsolutePathDecoded(this Uri uri)
        {
            return System.Web.HttpUtility.UrlDecode(uri.GetSafeAbsolutePath());
        }

        /// <summary>
        /// Rewrites the path of the uri so it ends with a slash.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <returns>The rewritten uri.</returns>
        /// <remarks>Everything else remains unchanged.</remarks>
        public static Uri EndPathWithSlash(this Uri uri)
        {
            var path = uri.GetSafeAbsolutePath();
            if (uri.IsAbsoluteUri)
            {
                if (path != "/" && path.EndsWith("/") == false)
                    uri = new Uri(uri.GetLeftPart(UriPartial.Authority) + path + "/" + uri.Query);
                return uri;
            }

            if (path != "/" && path.EndsWith("/") == false)
                    uri = new Uri(path + "/" + uri.Query, UriKind.Relative);

            return uri;
        }

        /// <summary>
        /// Rewrites the path of the uri so it does not end with a slash.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <returns>The rewritten uri.</returns>
        /// <remarks>Everything else remains unchanged.</remarks>
        public static Uri TrimPathEndSlash(this Uri uri)
        {
            var path = uri.GetSafeAbsolutePath();
            if (uri.IsAbsoluteUri)
            {
                if (path != "/")
                    uri = new Uri(uri.GetLeftPart(UriPartial.Authority) + path.TrimEnd('/') + uri.Query);
            }
            else
            {
                if (path != "/")
                    uri = new Uri(path.TrimEnd('/') + uri.Query, UriKind.Relative);
            }
            return uri;
        }

        /// <summary>
        /// Transforms a relative uri into an absolute uri.
        /// </summary>
        /// <param name="uri">The relative uri.</param>
        /// <param name="baseUri">The base absolute uri.</param>
        /// <returns>The absolute uri.</returns>
        public static Uri MakeAbsolute(this Uri uri, Uri baseUri)
        {
            if (uri.IsAbsoluteUri)
                throw new ArgumentException("Uri is already absolute.", "uri");

            return new Uri(baseUri.GetLeftPart(UriPartial.Authority) + uri.GetSafeAbsolutePath() + uri.GetSafeQuery());
        }

        static string GetSafeQuery(this Uri uri)
        {
            if (uri.IsAbsoluteUri)
                return uri.Query;

            // cannot get .Query on relative uri (InvalidOperation)
            var s = uri.OriginalString;
            var posq = s.IndexOf("?", StringComparison.Ordinal);
            var posf = s.IndexOf("#", StringComparison.Ordinal);
            var query = posq < 0 ? null : (posf < 0 ? s.Substring(posq) : s.Substring(posq, posf - posq));

            return query;
        }

        /// <summary>
        /// Removes the port from the uri.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <returns>The same uri, without its port.</returns>
        public static Uri WithoutPort(this Uri uri)
        {
            return new Uri(uri.GetComponents(UriComponents.AbsoluteUri & ~UriComponents.Port, UriFormat.UriEscaped));
        }

        /// <summary>
        /// Replaces the host of a uri.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <param name="host">A replacement host.</param>
        /// <returns>The same uri, with its host replaced.</returns>
        public static Uri ReplaceHost(this Uri uri, string host)
        {
            return new UriBuilder(uri) { Host = host }.Uri;
        }
    }
}
