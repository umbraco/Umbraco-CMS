using System;
using System.IO;
using System.Linq;
using System.Text;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;

namespace Umbraco.Core
{
    /// <summary>
    /// Provides extension methods to <see cref="Uri"/>.
    /// </summary>
    public static class UriExtensions
    {
        
        /// <summary>
        /// Checks if the current uri is a back office request
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        internal static bool IsBackOfficeRequest(this Uri url)
        {
        
            var authority = url.GetLeftPart(UriPartial.Authority);
            var afterAuthority = url.GetLeftPart(UriPartial.Query)
                                    .TrimStart(authority)
                                    .TrimStart("/");

            

            //check if this is in the umbraco back office
            return afterAuthority.InvariantStartsWith(GlobalSettings.Path.TrimStart("/"));
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
        /// This is a performance tweak to check if this is a .css, .js or .ico, .jpg, .jpeg, .png, .gif file request since
        /// .Net will pass these requests through to the module when in integrated mode.
        /// We want to ignore all of these requests immediately.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        internal static bool IsClientSideRequest(this Uri url)
        {
            var toIgnore = new[] { ".js", ".css", ".ico", ".png", ".jpg", ".jpeg", ".gif", ".html", ".svg" };
            return toIgnore.Any(x => Path.GetExtension(url.LocalPath).InvariantEquals(x));
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
            if (!path.StartsWith("/"))
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
            if (!path.StartsWith("/"))
                throw new ArgumentException("Path must start with a slash.", "path");
            if (query.Length > 0 && !query.StartsWith("?"))
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
                if (path != "/" && !path.EndsWith("/"))
                    uri = new Uri(uri.GetLeftPart(UriPartial.Authority) + path + "/" + uri.Query);
                return uri;
            }
            else
            {
                if (path != "/" && !path.EndsWith("/"))
                    uri = new Uri(path + "/" + uri.Query, UriKind.Relative);
            }
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
    }
}