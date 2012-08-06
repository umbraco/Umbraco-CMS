using System;
using System.Text;
using System.Web;

using Umbraco.Core;
using umbraco;

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
                _appVirtualPathPrefix = String.Empty;
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

		// fixme - what about vdir?
		// path = path.Substring(UriUtility.AppVirtualPathPrefix.Length); // remove virtual directory

    	public static Uri UriFromUmbraco(Uri uri)
    	{
    		var path = uri.GetSafeAbsolutePath();
    		if (path == "/")
    			return uri;

    		if (!GlobalSettings.UseDirectoryUrls)
    			path += ".aspx";
    		else if (UmbracoSettings.AddTrailingSlash)
    			path += "/";

    		return uri.Rewrite(path);
    	}

    	public static Uri UriToUmbraco(Uri uri)
    	{
    		var path = uri.GetSafeAbsolutePath();

    		path = path.ToLower();
    		if (path != "/")
    			path = path.TrimEnd('/');
    		if (path.EndsWith(".aspx"))
    			path = path.Substring(0, path.Length - ".aspx".Length);

    		return uri.Rewrite(path);
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
			return HasScheme(uri) ? uri : String.Format("{0}://{1}", scheme ?? Uri.UriSchemeHttp, uri);
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

    	/// <summary>
    	/// Returns an faull url with the host, port, etc...
    	/// </summary>
    	/// <param name="absolutePath">An absolute path (i.e. starts with a '/' )</param>
    	/// <param name="httpContext"> </param>
    	/// <returns></returns>
    	/// <remarks>
    	/// Based on http://stackoverflow.com/questions/3681052/get-absolute-url-from-relative-path-refactored-method
    	/// </remarks>
    	internal static Uri ToFullUrl(string absolutePath, HttpContextBase httpContext)
		{
    		if (httpContext == null) throw new ArgumentNullException("httpContext");
    		if (String.IsNullOrEmpty(absolutePath))
				throw new ArgumentNullException("absolutePath");
			
			if (!absolutePath.StartsWith("/"))
				throw new FormatException("The absolutePath specified does not start with a '/'");
			

			var url = httpContext.Request.Url;
			var port = url.Port != 80 ? (":" + url.Port) : String.Empty;

    		return new Uri(String.Format("{0}://{1}{2}{3}", url.Scheme, url.Host, port, absolutePath));
		}
    }
}