using System.Web;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Gets executed when no document can be found in Umbraco
	/// </summary>
	internal class DocumentNotFoundHandler : IHttpHandler
	{
		public void ProcessRequest(HttpContext context)
		{
			WriteOutput(context);
		}

		internal void WriteOutput(HttpContext context)
		{
			context.Response.StatusCode = 404;

			context.Response.Write("<html><body><h1>Page not found</h1>");
			UmbracoContext.Current.HttpContext.Response.Write("<h3>No umbraco document matches the url '" + HttpUtility.HtmlEncode(UmbracoContext.Current.ClientUrl) + "'.</h3>");

			// fixme - should try to get infos from the DocumentRequest?

			context.Response.Write("<p>This page can be replaced with a custom 404. Check the documentation for \"custom 404\".</p>");
			context.Response.Write("<p style=\"border-top: 1px solid #ccc; padding-top: 10px\"><small>This page is intentionally left ugly ;-)</small></p>");
			context.Response.Write("</body></html>");
		}

		public bool IsReusable
		{
			get { return false; }
		}
	}
}
