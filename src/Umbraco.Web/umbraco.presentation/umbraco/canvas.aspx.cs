using System;
using Umbraco.Core.Configuration;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.businesslogic.Exceptions;

namespace umbraco.presentation
{
    public partial class LiveEditingEnabler : UmbracoEnsuredPage
    {
        public LiveEditingEnabler()
        {
            CurrentApp = DefaultApps.content.ToString();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if ((UmbracoConfiguration.Current.UmbracoSettings.Content.EnableCanvasEditing || !String.IsNullOrEmpty(Request["umbSkinning"])) && getUser() != null)
            {
                UmbracoContext.Current.LiveEditingContext.Enabled = true;

                var redirUrl = "/";
                if (!string.IsNullOrEmpty(helper.Request("redir")))
                    redirUrl = helper.Request("redir");
                else if (Request.UrlReferrer != null && !Request.UrlReferrer.AbsolutePath.Contains("login.aspx"))
                    redirUrl = Request.UrlReferrer.AbsolutePath;

                Response.Redirect(redirUrl +
                    (string.IsNullOrEmpty(Request["umbSkinning"]) ? "" : "?umbSkinning=true") + (string.IsNullOrEmpty(Request["umbSkinningConfigurator"]) ? "" : "&umbSkinningConfigurator=true"), true);
            }
            else if (!UmbracoConfiguration.Current.UmbracoSettings.Content.EnableCanvasEditing)
            {
                throw new UserAuthorizationException(
                    "Canvas editing isn't enabled. It can be enabled via the UmbracoSettings.config");
            }
            else
            {
                throw new Exception("User not logged in");
            }
        }

        /// <summary>
        /// form1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.HtmlControls.HtmlForm form1;
    }
}
