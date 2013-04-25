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
using umbraco.BusinessLogic;

namespace umbraco.cms.presentation.settings.modal
{
    /// <summary>
    /// Summary description for ShowUmbracoTags.
    /// </summary>
    public partial class ShowUmbracoTags : BasePages.UmbracoEnsuredPage
    {

        public ShowUmbracoTags()
        {
            CurrentApp = DefaultApps.settings.ToString();
        }

        public static string alias = "";
        protected void Page_Load(object sender, EventArgs e)
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
        protected uicontrols.Pane Pane7;
    }
}
