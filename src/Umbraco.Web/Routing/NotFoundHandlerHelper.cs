using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Umbraco.Web.Routing
{
	// provides internal access to legacy url -- should get rid of it eventually
	internal static class NotFoundHandlerHelper
	{
		const string ContextKey = "Umbraco.Web.Routing.NotFoundHandlerHelper.Url";

		public static string GetLegacyUrlForNotFoundHandlers()
		{
			// that's not backward-compatible because when requesting "/foo.aspx"
			// 4.9  : url = "foo.aspx"
			// 4.10 : url = "/foo"
			//return pcr.Uri.AbsolutePath;

			// so we have to run the legacy code for url preparation :-(

			var httpContext = HttpContext.Current;

			if (httpContext == null)
				return "";

			var url = httpContext.Items[ContextKey] as string;
			if (url != null)
				return url;

			// code from requestModule.UmbracoRewrite
			string tmp = httpContext.Request.Path.ToLower();

			// note: requestModule.UmbracoRewrite also does some confusing stuff
			// with stripping &umbPage from the querystring?! ignored.

			// code from requestHandler.cleanUrl
			string root = Umbraco.Core.IO.SystemDirectories.Root.ToLower();
			if (!string.IsNullOrEmpty(root) && tmp.StartsWith(root))
				tmp = tmp.Substring(root.Length);
			tmp = tmp.TrimEnd('/');
			if (tmp == "/default.aspx")
				tmp = string.Empty;
			else if (tmp == root)
				tmp = string.Empty;

			// code from UmbracoDefault.Page_PreInit
			if (tmp != "" && httpContext.Request["umbPageID"] == null)
			{
				string tryIntParse = tmp.Replace("/", "").Replace(".aspx", string.Empty);
				int result;
				if (int.TryParse(tryIntParse, out result))
					tmp = tmp.Replace(".aspx", string.Empty);
			}
			else if (!string.IsNullOrEmpty(httpContext.Request["umbPageID"]))
			{
				int result;
				if (int.TryParse(httpContext.Request["umbPageID"], out result))
				{
					tmp = httpContext.Request["umbPageID"];
				}
			}

			// code from requestHandler.ctor
			if (tmp != "")
				tmp = tmp.Substring(1);

			httpContext.Items[ContextKey] = tmp;
			return tmp;
		}
	}
}
