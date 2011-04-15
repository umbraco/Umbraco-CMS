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
using System.Xml;
using System.Xml.XPath;
using umbraco.IO;

namespace umbraco.presentation.developer.packages {
    public partial class BrowseRepository : BasePages.UmbracoEnsuredPage {

        public BrowseRepository()
        {
            CurrentApp = BusinessLogic.DefaultApps.developer.ToString();

        }

        protected void Page_Load(object sender, System.EventArgs e) {

            Exception ex = new Exception();
            if (!cms.businesslogic.packager.Settings.HasFileAccess(ref ex)) {
                fb.Style.Add("margin-top", "7px");
                fb.type = global::umbraco.uicontrols.Feedback.feedbacktype.error;
                fb.Text = "<strong>" + ui.Text("errors", "filePermissionsError") + ":</strong><br/>" + ex.Message;
            }

            string category = Request.QueryString["category"];
            string repoGuid = Request.QueryString["repoGuid"];

            cms.businesslogic.packager.repositories.Repository repo = cms.businesslogic.packager.repositories.Repository.getByGuid(repoGuid);
            string url = repo.RepositoryUrl;

            Panel1.Text = "Browse repository: " + repo.Name;
            
            if (!string.IsNullOrEmpty(category))
                category = "&category=" + category;

            iframeGen.Text = "<iframe id=\"repoFrame\" frameborder=\"1\" style=\"border: none; display: block\" src=\"" + url + "?repoGuid=" + repoGuid + category + "&callback=" + Request.ServerVariables["SERVER_NAME"] + ":" + Request.ServerVariables["SERVER_PORT"] + IOHelper.ResolveUrl( SystemDirectories.Umbraco ) + "/developer/packages/proxy.htm?/" + IOHelper.ResolveUrl(SystemDirectories.Umbraco).Trim('/') + "/developer/packages/installer.aspx?repoGuid=" + repoGuid + "&version=v45&useLegacySchema=" + UmbracoSettings.UseLegacyXmlSchema.ToString() +  "\"></iframe>";
        }

        #region Web Form Designer generated code
        override protected void OnInit(EventArgs e) {
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
        private void InitializeComponent() {

        }
        #endregion

    }
}
