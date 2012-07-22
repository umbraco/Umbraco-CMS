using System;
using System.Text;
using System.Web;

using Umbraco.Core;

namespace Umbraco.Web
{
    static class UriUtility
    {
        static readonly string _appVirtualPath;
        static readonly string _appVirtualPathPrefix;

        static UriUtility()
        {
			// Virtual path
            _appVirtualPath = HttpRuntime.AppDomainAppVirtualPath ?? "/";
            _appVirtualPathPrefix = _appVirtualPath;
            if (_appVirtualPathPrefix == "/")
                _appVirtualPathPrefix = string.Empty;
        }

		// will be "/" or "/foo"
        public static string AppVirtualPath
        {
            get { return _appVirtualPath; }
        }

		// will be "" or "/foo"
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

		#region Uri string utilities

		public static bool HasScheme(string uri)
		{
			return uri.IndexOf("://") > 0;
		}

		public static string StartWithScheme(string uri)
		{
			return StartWithScheme(uri, null);
		}

		public static string StartWithScheme(string uri, string scheme)
		{
			return HasScheme(uri) ? uri : string.Format("{0}://{1}", scheme ?? Uri.UriSchemeHttp, uri);
		}

		public static string EndPathWithSlash(string uri)
		{
			var pos1 = Math.Max(0, uri.IndexOf('?'));
			var pos2 = Math.Max(0, uri.IndexOf('#'));
			var pos = Math.Min(pos1, pos2);

			var path = pos > 0 ? uri.Substring(0, pos) : uri;
			path = path.EnsureEndsWith('/');

			if (pos > 0)
				path += uri.Substring(pos);

			return path;
		}

		public static string TrimPathEndSlash(string uri)
		{
			var pos1 = Math.Max(0, uri.IndexOf('?'));
			var pos2 = Math.Max(0, uri.IndexOf('#'));
			var pos = Math.Min(pos1, pos2);

			var path = pos > 0 ? uri.Substring(0, pos) : uri;
			path = path.TrimEnd('/');

			if (pos > 0)
				path += uri.Substring(pos);

			return path;
		}

		#endregion
    }
}