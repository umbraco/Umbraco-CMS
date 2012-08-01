using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace umbraco.presentation.actions
{

    /// <summary>
    /// This page is used only to deeplink to the edit content page with the tree
    /// </summary>
    public partial class editContent : BasePages.UmbracoEnsuredPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Redirect("../umbraco.aspx?app=content&rightAction=editContent&id=" + helper.Request("id") + "#content", true);
        }
    }
}
