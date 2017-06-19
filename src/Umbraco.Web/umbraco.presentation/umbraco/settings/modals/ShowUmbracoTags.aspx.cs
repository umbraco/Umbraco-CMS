using System;
using Umbraco.Core;

namespace umbraco.cms.presentation.settings.modal
{
    /// <summary>
    /// Summary description for ShowUmbracoTags.
    /// </summary>
    public partial class ShowUmbracoTags : Umbraco.Web.UI.Pages.UmbracoEnsuredPage
    {

        public ShowUmbracoTags()
        {
            CurrentApp = Constants.Applications.Settings.ToString();
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
        protected Umbraco.Web._Legacy.Controls.Pane Pane7;
    }
}
