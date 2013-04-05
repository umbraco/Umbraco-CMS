using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace umbraco.cms.presentation.settings.modal
{
    /// <summary>
    /// Summary description for ShowUmbracoTags.
    /// </summary>
    public partial class ShowUmbracoTags : umbraco.BasePages.UmbracoEnsuredPage
    {

        public static string alias = "";
        protected void Page_Load(object sender, System.EventArgs e)
        {
            alias = Request.QueryString["alias"].Replace(" ", "").Trim();
            // Put user code to initialize the page here
        }

        /// <summary>
        /// Pane7 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Pane Pane7;
    }
}
