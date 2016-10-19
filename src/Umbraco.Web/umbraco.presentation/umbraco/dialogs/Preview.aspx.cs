using System;
using System.Web;
using Umbraco.Web;
using Umbraco.Core;

namespace umbraco.presentation.dialogs
{
    public partial class Preview : Umbraco.Web.UI.Pages.UmbracoEnsuredPage
    {
        public Preview()
        {
            CurrentApp = Constants.Applications.Content;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var user = UmbracoContext.Security.CurrentUser;
            var contentId = Request.GetItemAs<int>("id");

            var facadeService = Current.FacadeService;
            var previewToken = facadeService.EnterPreview(user, contentId);

            UmbracoContext.HttpContext.Response.Cookies.Set(new HttpCookie(Constants.Web.PreviewCookieName, previewToken));

            // use a numeric url because content may not be in cache and so .Url would fail
            Response.Redirect($"../../{contentId}.aspx", true);
        }
    }
}
