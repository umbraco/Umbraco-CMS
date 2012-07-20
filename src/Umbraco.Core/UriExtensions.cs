using System;
using System.Text;

namespace Umbraco.Core
{
    internal static class UriExtensions
    {
		// Creates a new Uri with a rewritten path
		// Everything else is unchanged but for the fragment which is lost
		public static Uri Rewrite(this Uri uri, string path)
		{
			if (uri.IsAbsoluteUri)
			{
				return new Uri(uri.GetLeftPart(UriPartial.Authority) + path + uri.Query);
			}
			else
			{
				// cannot get .Query on relative uri (InvalidOperation)
				var s = uri.OriginalString;
				var posq = s.IndexOf("?");
				var posf = s.IndexOf("#");
				var query = posq < 0 ? null : (posf < 0 ? s.Substring(posq) : s.Substring(posq, posf - posq));

				return new Uri(path + query, UriKind.Relative);
			}
		}

		// Creates a new Uri with a rewritten path and query
		// Everything else is unchanged but for the fragment which is lost
		public static Uri Rewrite(this Uri uri, string path, string query)
		{
			if (uri.IsAbsoluteUri)
			{
				return new Uri(uri.GetLeftPart(UriPartial.Authority) + path + query);
			}
			else
			{
				return new Uri(path + query, UriKind.Relative);
			}
		}

		// Gets the absolute path of the Uri
		// Works both for Absolute and Relative Uris
		public static string GetSafeAbsolutePath(this Uri uri)
		{
			if (uri.IsAbsoluteUri)
			{
				return uri.AbsolutePath;
			}
			else
			{
				// cannot get .AbsolutePath on relative uri (InvalidOperation)
				var s = uri.OriginalString;
				var posq = s.IndexOf("?");
				var posf = s.IndexOf("#");
				var pos = posq > 0 ? posq : (posf > 0 ? posf : 0);
				var path = pos > 0 ? s.Substring(0, pos) : s;
				return path;
			}
		}

		// Creates a new Uri with path ending with a slash
		// Everything else is unchanged but for the fragment which is lost
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

		// Creates a new Uri with path trimmed of trailing slash
		// Everything else is unchanged but for the fragment which is lost
		// If path is "/" it remains "/".
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
	}
}