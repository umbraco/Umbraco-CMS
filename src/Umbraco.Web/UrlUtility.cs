using System;
using System.Text;
using System.Web;

namespace Umbraco.Web
{
    static class UrlUtility
    {
        static readonly string _appVirtualPath;
        static readonly string _appVirtualPathPrefix;

        static UrlUtility()
        {
            _appVirtualPath = HttpRuntime.AppDomainAppVirtualPath ?? "/";
            _appVirtualPathPrefix = _appVirtualPath;
            if (_appVirtualPathPrefix == "/")
                _appVirtualPathPrefix = string.Empty;
        }

        public static string AppVirtualPath
        {
            get { return _appVirtualPath; }
        }

        public static string AppVirtualPathPrefix
        {
            get { return _appVirtualPathPrefix; }
        }

        public static string ToAbsolute(string url)
        {
            return ResolveUrl(url);
        }

        public static string ToAppRelative(string url)
        {
            if (url.StartsWith(_appVirtualPathPrefix))
                url = url.Substring(_appVirtualPathPrefix.Length);
            return url;
        }

        #region ResolveUrl

        // http://www.codeproject.com/Articles/53460/ResolveUrl-in-ASP-NET-The-Perfect-Solution
        // note
        // if browsing http://example.com/sub/page1.aspx then 
        // ResolveUrl("page2.aspx") returns "/page2.aspx"
        // Page.ResolveUrl("page2.aspx") returns "/sub/page2.aspx" (relative...)
        //
        public static string ResolveUrl(string relativeUrl)
        {
            if (relativeUrl == null) throw new ArgumentNullException("relativeUrl");

            if (relativeUrl.Length == 0 || relativeUrl[0] == '/' || relativeUrl[0] == '\\')
                return relativeUrl;

            int idxOfScheme = relativeUrl.IndexOf(@"://", StringComparison.Ordinal);
            if (idxOfScheme != -1)
            {
                int idxOfQM = relativeUrl.IndexOf('?');
                if (idxOfQM == -1 || idxOfQM > idxOfScheme) return relativeUrl;
            }

            StringBuilder sbUrl = new StringBuilder();
            sbUrl.Append(HttpRuntime.AppDomainAppVirtualPath);
            if (sbUrl.Length == 0 || sbUrl[sbUrl.Length - 1] != '/') sbUrl.Append('/');

            // found question mark already? query string, do not touch!
            bool foundQM = false;
            bool foundSlash; // the latest char was a slash?
            if (relativeUrl.Length > 1
                && relativeUrl[0] == '~'
                && (relativeUrl[1] == '/' || relativeUrl[1] == '\\'))
            {
                relativeUrl = relativeUrl.Substring(2);
                foundSlash = true;
            }
            else foundSlash = false;
            foreach (char c in relativeUrl)
            {
                if (!foundQM)
                {
                    if (c == '?') foundQM = true;
                    else
                    {
                        if (c == '/' || c == '\\')
                        {
                            if (foundSlash) continue;
                            else
                            {
                                sbUrl.Append('/');
                                foundSlash = true;
                                continue;
                            }
                        }
                        else if (foundSlash) foundSlash = false;
                    }
                }
                sbUrl.Append(c);
            }

            return sbUrl.ToString();
        }

        #endregion

        #region Utilities

        public static bool HasScheme(string uri)
        {
            return uri.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase)
                   || uri.StartsWith("https://", StringComparison.CurrentCultureIgnoreCase);
        }

        public static string EnsureScheme(string uri, string scheme)
        {
            return HasScheme(uri) ? uri : string.Format("{0}://{1}", scheme, uri);
        }

        public static string WithTrailingSlash(string uri)
        {
            return uri.EndsWith("/") ? uri : uri + "/";
        }

        // indicates whether uri2 is within uri1
        public static bool IsBaseOf(string uri1, string uri2)
        {
            uri2 = WithTrailingSlash(uri2);
            Uri testUri2 = new Uri(uri2);

            uri1 = WithTrailingSlash(uri1);
            uri1 = EnsureScheme(uri1, testUri2.Scheme);
            Uri testUri1 = new Uri(uri1);

            return testUri1.IsBaseOf(testUri2);
        }

        public static bool IsBaseOf(string uri1, Uri uri2)
        {
            return IsBaseOf(uri1, uri2.ToString());
        }

        #endregion
    }
}