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

namespace umbraco.cms.presentation
{
    /// <summary>
    /// Summary description for publishStatus.
    /// </summary>
    [Obsolete("This class is no longer used and will be removed from the codebase in future versions")]
    public partial class publishStatus : BasePages.UmbracoEnsuredPage
    {
        public publishStatus()
        {
            CurrentApp = BusinessLogic.DefaultApps.content.ToString();

        }

        protected void Page_Load(object sender, System.EventArgs e)
        {
            // Put user code to initialize the page here
            int totalNodes = cms.businesslogic.CMSNode.CountByObjectType(cms.businesslogic.web.Document._objectType);
            if (library.IsPublishing)
            {
                Panel1.Controls.Add(new LiteralControl("Der er nu publiseret " + library.NodesPublished.ToString() + " ud af " + totalNodes.ToString() + " noder..."));
                base.RefreshPage(2);
            }
            else
                Panel1.Controls.Add(new LiteralControl("Færdig. Der er ialt publiseret " + library.NodesPublished.ToString() + " noder og det tog " + ((long)((System.DateTime.Now.Ticks - library.PublishStart.Ticks) / 10000000)).ToString() + " sekunder..."));
        }

        #region Web Form Designer generated code
        override protected void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {

        }
        #endregion

        /// <summary>
        /// Form1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.HtmlControls.HtmlForm Form1;

        /// <summary>
        /// Panel1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.UmbracoPanel Panel1;
    }
}
