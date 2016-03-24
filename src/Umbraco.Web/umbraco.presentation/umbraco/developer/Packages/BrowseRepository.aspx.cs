using System;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Web;
using Umbraco.Web.UI.Pages;

namespace umbraco.presentation.developer.packages {
    public partial class BrowseRepository : UmbracoEnsuredPage {

        public BrowseRepository()
        {
            CurrentApp = Constants.Applications.Developer.ToString();

        }

        protected void Page_Load(object sender, System.EventArgs e) {

            Exception ex = new Exception();
            if (!cms.businesslogic.packager.Settings.HasFileAccess(ref ex)) {
                fb.Style.Add("margin-top", "7px");
                fb.type = global::umbraco.uicontrols.Feedback.feedbacktype.error;
                fb.Text = "<strong>" + Services.TextService.Localize("errors/filePermissionsError") + ":</strong><br/>" + ex.Message;
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
                    "<iframe id=\"repoFrame\" frameborder=\"1\" style=\"border: none; display: block\" src=\"{0}?repoGuid={1}{2}&callback={3}:{4}{5}/developer/packages/proxy.htm?/{6}/developer/packages/installer.aspx?repoGuid={7}&version=v45&fullVersion={8}.{9}.{10}&dotnetVersion={11}&trustLevel={12}\"></iframe>",
                    url, repoGuid, category, Request.ServerVariables["SERVER_NAME"],
                    Request.ServerVariables["SERVER_PORT"], IOHelper.ResolveUrl(SystemDirectories.Umbraco),
                    IOHelper.ResolveUrl(SystemDirectories.Umbraco).Trim('/'), repoGuid,
                    UmbracoVersion.Current.Major,
                    UmbracoVersion.Current.Minor,
                    UmbracoVersion.Current.Build,
                    Environment.Version,
                    Umbraco.Core.SystemUtilities.GetCurrentTrustLevel());
        }


    }
}
