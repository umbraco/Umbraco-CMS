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
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Web;

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

            string category = Request.CleanForXss("category");
            string repoGuid = Request.CleanForXss("repoGuid");

            var repo = cms.businesslogic.packager.repositories.Repository.getByGuid(repoGuid);
            if (repo == null)
            {
                throw new InvalidOperationException("Could not find repository with id " + repoGuid);
            }

            string url = repo.RepositoryUrl;

            Panel1.Text = "Browse repository: " + repo.Name;
            
            if (!string.IsNullOrEmpty(category))
                category = "&category=" + category;

            iframeGen.Text =
                string.Format(
                    "<iframe id=\"repoFrame\" frameborder=\"1\" style=\"border: none; display: block\" src=\"{0}?repoGuid={1}{2}&callback={3}:{4}{5}/developer/packages/proxy.htm?/{6}/developer/packages/installer.aspx?repoGuid={7}&version=v45&fullVersion={8}.{9}.{10}&useLegacySchema={11}&dotnetVersion={12}&trustLevel={13}\"></iframe>",
                    url, repoGuid, category, Request.ServerVariables["SERVER_NAME"],
                    Request.ServerVariables["SERVER_PORT"], IOHelper.ResolveUrl(SystemDirectories.Umbraco),
                    IOHelper.ResolveUrl(SystemDirectories.Umbraco).Trim('/'), repoGuid,
                    UmbracoVersion.Current.Major,
                    UmbracoVersion.Current.Minor,
                    UmbracoVersion.Current.Build,
                    UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema.ToString(), Environment.Version,
                    Umbraco.Core.SystemUtilities.GetCurrentTrustLevel());
        }


    }
}
