using System;
using System.Web;
using System.Web.UI;
using Umbraco.Core;
using Umbraco.Web;
using Umbraco.Web.Composing;
using Umbraco.Web.PublishedCache;

namespace umbraco.presentation
{
    public class endPreview : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var request = (new HttpRequestWrapper(Request));

            var previewToken = request.GetPreviewCookieValue();
            var service = Current.PublishedSnapshotService;
            service.ExitPreview(previewToken);

            HttpContext.Current.ExpireCookie(Constants.Web.PreviewCookieName);

            var redir = Request.QueryString["redir"];
            Uri url = null;

            if (Uri.IsWellFormedUriString(redir, UriKind.Relative) == false
                || redir.StartsWith("//")
                || Uri.TryCreate(redir, UriKind.Relative, out url) == false)
            {
                Response.Redirect("/", true);
            }

            Response.Redirect(url.ToString(), true);
        }
    }
}
