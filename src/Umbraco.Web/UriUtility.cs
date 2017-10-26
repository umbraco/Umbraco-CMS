using System;
using System.Text;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using GlobalSettings = umbraco.GlobalSettings;

namespace Umbraco.Web
{
    public static class UriUtility
    {
        static string _appPath;
        static string _appPathPrefix;

        static UriUtility()
        {
			SetAppDomainAppVirtualPath(HttpRuntime.AppDomainAppVirtualPath);
        }

		// internal for unit testing only
		internal static void SetAppDomainAppVirtualPath(string appPath)
		{
			_appPath = appPath ?? "/";
			_appPathPrefix = _appPath;
			if (_appPathPrefix == "/")
				_appPathPrefix = String.Empty;
		}

		// will be "/" or "/foo"
        public static string AppPath
        {
            get { return _appPath; }
        }

		// will be "" or "/foo"
        public static string AppPathPrefix
        {
            get { return _appPathPrefix; }
        }

		// adds the virtual directory if any
		// see also VirtualPathUtility.ToAbsolute
		public static string ToAbsolute(string url)
        {
			//return ResolveUrl(url);
			url = url.TrimStart('~');
			return _appPathPrefix + url;
        }

		// strips the virtual directory if any
		// see also VirtualPathUtility.ToAppRelative
        public static string ToAppRelative(string virtualPath)
        {
            if (virtualPath.InvariantStartsWith(_appPathPrefix)
                    && (virtualPath.Length == _appPathPrefix.Length || virtualPath[_appPathPrefix.Length] == '/'))
                virtualPath = virtualPath.Substring(_appPathPrefix.Length);
			if (virtualPath.Length == 0)
				virtualPath = "/";
            return virtualPath;
        }

		// maps an internal umbraco uri to a public uri
		// ie with virtual directory, .aspx if required...
    	public static Uri UriFromUmbraco(Uri uri)
    	{
    		var path = uri.GetSafeAbsolutePath();

			if (path != "/")
			{
				if (!GlobalSettings.UseDirectoryUrls)
					path += ".aspx";
                else if (UmbracoConfig.For.UmbracoSettings().RequestHandler.AddTrailingSlash)
				    path = path.EnsureEndsWith("/");
			}

			path = ToAbsolute(path);

    		return uri.Rewrite(path);
    	}

		// maps a public uri to an internal umbraco uri
		// ie no virtual directory, no .aspx, lowercase...
		public static Uri UriToUmbraco(Uri uri)
    	{
			// note: no need to decode uri here because we're returning a uri
			// so it will be re-encoded anyway
    		var path = uri.GetSafeAbsolutePath();

    		path = path.ToLower();
			path = ToAppRelative(path); // strip vdir if any

			//we need to check if the path is /default.aspx because this will occur when using a 
			//web server pre IIS 7 when requesting the root document
			//if this is the case we need to change it to '/'
			if (path.StartsWith("/default.aspx", StringComparison.InvariantCultureIgnoreCase))
			{
				string rempath = path.Substring("/default.aspx".Length, path.Length - "/default.aspx".Length);
				path = rempath.StartsWith("/") ? rempath : "/" + rempath;
			}
			if (path != "/")
			{
				path = path.TrimEnd('/');
			}

			//if any part of the path contains .aspx, replace it with nothing.
			//sometimes .aspx is not at the end since we might have /home/sub1.aspx/customtemplate
			path = path.Replace(".aspx", "");

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
            sbUrl.Append(_appPathPrefix);
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

			return new Uri(absolutePath, UriKind.Relative).MakeAbsolute(httpContext.Request.Url);
		}
    }
}