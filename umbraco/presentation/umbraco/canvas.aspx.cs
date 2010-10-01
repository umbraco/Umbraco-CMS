using System;
using umbraco.BasePages;

namespace umbraco.presentation
{
    public partial class LiveEditingEnabler : BasePages.UmbracoEnsuredPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (base.getUser() != null)
            {
                UmbracoContext.Current.LiveEditingContext.Enabled = true;

                string redirUrl = "/";
                if (!String.IsNullOrEmpty(helper.Request("redir")))
                    redirUrl = helper.Request("redir");
                else if (Request.UrlReferrer != null && !Request.UrlReferrer.AbsolutePath.Contains("login.aspx"))
                    redirUrl = Request.UrlReferrer.AbsolutePath;

                Response.Redirect(redirUrl +
                    (string.IsNullOrEmpty(UmbracoContext.Current.Request["umbSkinning"]) ? "" : "?umbSkinning=true") + (string.IsNullOrEmpty(UmbracoContext.Current.Request["umbSkinningConfigurator"]) ? "" : "&umbSkinningConfigurator=true"), true);
            }
            else
            {
                throw new Exception("User not logged in");
            }
        }
    }
}
