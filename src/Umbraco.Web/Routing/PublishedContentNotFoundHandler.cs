using System.Web;
using Umbraco.Web.Composing;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Gets executed when no document can be found in Umbraco
    /// </summary>
    internal class PublishedContentNotFoundHandler : IHttpHandler
    {
        private readonly string _message;

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

            var frequest = Current.UmbracoContext.PublishedRequest;
            var reason = "Cannot render the page at URL '{0}'.";
            if (frequest.HasPublishedContent == false)
                reason = "No umbraco document matches the URL '{0}'.";
            else if (frequest.HasTemplate == false)
                reason = "No template exists to render the document at URL '{0}'.";

            response.Write("<html><body><h1>Page not found</h1>");
            response.Write("<h2>");
            response.Write(string.Format(reason, HttpUtility.HtmlEncode(Current.UmbracoContext.OriginalRequestUrl.PathAndQuery)));
            response.Write("</h2>");
            if (string.IsNullOrWhiteSpace(_message) == false)
                response.Write("<p>" + _message + "</p>");
            response.Write("<p>This page can be replaced with a custom 404. Check the documentation for <a href=\"https://our.umbraco.com/Documentation/Tutorials/Custom-Error-Pages/#404-errors\" target=\"_blank\">Custom 404 Error Pages</a>.</p>");
            response.Write("<p style=\"border-top: 1px solid #ccc; padding-top: 10px\"><small>This page is intentionally left ugly ;-)</small></p>");
            response.Write("</body></html>");
        }

        public bool IsReusable => false;
    }
}
