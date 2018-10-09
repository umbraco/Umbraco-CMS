using System;
using System.Web.UI;
using umbraco.cms.presentation.Trees;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Web;
using Umbraco.Web.UI.JavaScript;
using Umbraco.Web._Legacy.Controls;
using Umbraco.Web.UI.Pages;

namespace umbraco.presentation.developer.packages
{
    /// <summary>	
    /// Summary description for packager.	
    /// </summary>	
    [Obsolete("This should not be used and will be removed in v8, this is kept here only for backwards compat reasons, this page should never be rendered/used")]
    public class Installer : UmbracoEnsuredPage
    {

        private Control _configControl;
        private readonly cms.businesslogic.packager.Installer _installer;
        protected Pane pane_installing;
        protected Pane pane_optional;

        public Installer()
        {
            CurrentApp = Constants.Applications.Packages;
            _installer = new cms.businesslogic.packager.Installer(Security.CurrentUser.Id);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Request.GetItemAsString("installing")))
                return;

            pane_optional.Visible = false;
            pane_installing.Visible = true;
            ProcessInstall(Request.GetItemAsString("installing"));
        }

        private void ProcessInstall(string currentStep)
        {
            var dir = Request.GetItemAsString("dir");

            int.TryParse(Request.GetItemAsString("pId"), out var packageId);

            switch (currentStep.ToLowerInvariant())
            {
                case "custominstaller":
                    var customControl = Request.GetItemAsString("customControl");

                    if (customControl.IsNullOrWhiteSpace() == false)
                    {
                        pane_optional.Visible = false;

                        _configControl = LoadControl(SystemDirectories.Root + customControl);
                        _configControl.ID = "packagerConfigControl";

                        pane_optional.Controls.Add(_configControl);
                        pane_optional.Visible = true;

                        if (IsPostBack == false)
                        {
                            //We still need to clean everything up which is normally done in the Finished Action
                            PerformPostInstallCleanup(packageId, dir);
                        }

                    }
                    else
                    {
                        //if the custom installer control is empty here (though it should never be because we've already checked for it previously)
                        //then we should run the normal FinishedAction
                        PerformFinishedAction(packageId, dir);
                    }
                    break;
                default:
                    break;
            }
        }

        private void PerformPostInstallCleanup(int packageId, string dir)
        {
            _installer.InstallCleanUp(packageId, dir);

            // Update ClientDependency version
            var clientDependencyConfig = new ClientDependencyConfiguration(Logger);
            var clientDependencyUpdated = clientDependencyConfig.UpdateVersionNumber(
                UmbracoVersion.SemanticVersion, DateTime.UtcNow, "yyyyMMdd");

            //clear the tree cache - we'll do this here even though the browser will reload, but just in case it doesn't can't hurt.
            ClientTools.ClearClientTreeCache().RefreshTree("packager");
            TreeDefinitionCollection.Instance.ReRegisterTrees();
        }

        private void PerformFinishedAction(int packageId, string dir)
        {
            pane_optional.Visible = false;
            PerformPostInstallCleanup(packageId, dir);
        }
    }
}
