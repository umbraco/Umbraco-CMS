using System;
using System.Web;
using System.Web.SessionState;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.BaseRest;

namespace Umbraco.Web.BaseRest
{
    [Obsolete("Umbraco /base is obsoleted, use WebApi (UmbracoApiController) instead for all REST based logic")]
	internal class BaseRestHandler : IHttpHandler, IRequiresSessionState
	{
		static readonly string BaseUrl;

		static BaseRestHandler()
		{
			BaseUrl = UriUtility.ToAbsolute(Core.IO.SystemDirectories.Base).ToLower();
			if (!BaseUrl.EndsWith("/"))
				BaseUrl += "/";
		}

		public bool IsReusable
		{
			get { return true; }
		}

		/// <summary>
		/// Returns a value indicating whether a specified Uri should be routed to the BaseRestHandler.
		/// </summary>
		/// <param name="uri">The specified Uri.</param>
		/// <returns>A value indicating whether the specified Uri should be routed to the BaseRestHandler.</returns>
		public static bool IsBaseRestRequest(Uri uri)
		{
		    if (uri == null) return false;
            return UmbracoConfig.For.BaseRestExtensions().Enabled
				&& uri.AbsolutePath.ToLowerInvariant().StartsWith(BaseUrl);
		}

		public void ProcessRequest(HttpContext context)
		{
			string url = context.Request.RawUrl;

			// sanitize and split the url
			url = url.Substring(BaseUrl.Length);
			if (url.ToLower().Contains(".aspx"))
				url = url.Substring(0, url.IndexOf(".aspx", StringComparison.OrdinalIgnoreCase));
			if (url.ToLower().Contains("?"))
				url = url.Substring(0, url.IndexOf("?", StringComparison.OrdinalIgnoreCase));
			var urlParts = url.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

			// by default, return xml content
			context.Response.ContentType = "text/xml";

			// ensure that we have a valid request ie /base/library/method/[parameters].aspx
			if (urlParts.Length < 2)
			{
				context.Response.Write("<error>Invalid request, missing parts.</error>");
				context.Response.StatusCode = 400;
				context.Response.StatusDescription = "Bad Request";
				context.Response.End();
				return;
			}

			var extensionAlias = urlParts[0];
			var methodName = urlParts[1];
		    var paramsCount = urlParts.Length - 2;

			var method = RestExtensionMethodInfo.GetMethod(extensionAlias, methodName, paramsCount);

			if (!method.Exists)
			{
				context.Response.StatusCode = 500;
				context.Response.StatusDescription = "Internal Server Error";
				context.Response.Output.Write("<error>Extension or method not found.</error>");
			}
			else if (!method.CanBeInvokedByCurrentMember)
			{
				context.Response.StatusCode = 500;
				context.Response.StatusDescription = "Internal Server Error";
				context.Response.Output.Write("<error>Permission denied.</error>");
			}
			else
			{
				if (!method.ReturnXml)
					context.Response.ContentType = "text/html";

				TrySetCulture();

				var result = method.Invoke(urlParts.Skip(2).ToArray());
				if (result.Length >= 7 && result.Substring(0, 7) == "<error>")
				{
					context.Response.StatusCode = 500;
					context.Response.StatusDescription = "Internal Server Error";
				}
				context.Response.Output.Write(result);
			}

			context.Response.End();
		}

		#region from baseHttpModule.cs

		// note - is this ok?

		private static void TrySetCulture()
		{
			var domain = HttpContext.Current.Request.Url.Host; // host only
			if (TrySetCulture(domain)) return;

			domain = HttpContext.Current.Request.Url.Authority; // host with port
		    TrySetCulture(domain);
		}

		private static bool TrySetCulture(string domain)
		{
			var uDomain = global::umbraco.cms.businesslogic.web.Domain.GetDomain(domain);
			if (uDomain == null) return false;
			System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(uDomain.Language.CultureAlias);
			return true;
		}

		#endregion
	}
}
