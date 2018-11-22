using System.Web;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Gets executed when no document can be found in Umbraco
	/// </summary>
	internal class PublishedContentNotFoundHandler : IHttpHandler
	{
		string _message;

		public PublishedContentNotFoundHandler()
		{ }

		public PublishedContentNotFoundHandler(string message)
		{
			_message = message;
		}

		public void ProcessRequest(HttpContext context)
		{
			WriteOutput(context);
		}

		internal void WriteOutput(HttpContext context)
		{
			var response = context.Response;

			response.Clear();

			var docreq = UmbracoContext.Current.PublishedContentRequest;
			var reason = "Cannot render the page at url '{0}'.";
			if (!docreq.HasPublishedContent)
				reason = "No umbraco document matches the url '{0}'.";
			else if (!docreq.HasTemplate)
				reason = "No template exists to render the document at url '{0}'.";

			response.Write("<html><body><h1>Page not found</h1>");
			response.Write("<h3>");
			response.Write(string.Format(reason, HttpUtility.HtmlEncode(UmbracoContext.Current.OriginalRequestUrl.PathAndQuery)));
			response.Write("</h3>");
			if (!string.IsNullOrWhiteSpace(_message))
				response.Write("<p>" + _message + "</p>");
			response.Write("<p>This page can be replaced with a custom 404. Check the documentation for \"custom 404\".</p>");
			response.Write("<p style=\"border-top: 1px solid #ccc; padding-top: 10px\"><small>This page is intentionally left ugly ;-)</small></p>");
			response.Write("</body></html>");
		}

		public bool IsReusable
		{
			get { return false; }
		}
	}
}
