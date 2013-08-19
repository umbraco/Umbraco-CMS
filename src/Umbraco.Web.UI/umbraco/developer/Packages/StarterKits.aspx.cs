using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Umbraco.Core.IO;
using Umbraco.Web.UI.Install.Steps.Skinning;
using Umbraco.Web.UI.Pages;

namespace Umbraco.Web.UI.Umbraco.Developer.Packages
{
    using System.IO;

    public partial class StarterKits : UmbracoEnsuredPage
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!global::umbraco.cms.businesslogic.skinning.Skinning.IsStarterKitInstalled())
                ShowStarterKits();
            else
                ShowSkins((Guid)global::umbraco.cms.businesslogic.skinning.Skinning.StarterKitGuid());
        }

        private void ShowStarterKits()
        {
            if (Directory.Exists(this.Server.MapPath(SystemDirectories.Install)) == false)
            {
                this.InstallationDirectoryNotAvailable.Visible = true;
                StarterKitNotInstalled.Visible = false;
                StarterKitInstalled.Visible = false;
            
                return;
            }


            var starterkitsctrl = (LoadStarterKits)LoadControl(SystemDirectories.Install + "/steps/Skinning/loadStarterKits.ascx");
            starterkitsctrl.StarterKitInstalled += StarterkitsctrlStarterKitInstalled;

            ph_starterkits.Controls.Add(starterkitsctrl);

            StarterKitNotInstalled.Visible = true;
            StarterKitInstalled.Visible = false;
            InstallationDirectoryNotAvailable.Visible = true;

        }

        public void ShowSkins(Guid starterKitGuid)
        {

            var ctrl = (LoadStarterKitDesigns)LoadControl(SystemDirectories.Install + "/steps/Skinning/loadStarterKitDesigns.ascx");
            ctrl.ID = "StarterKitDesigns";

            ctrl.StarterKitGuid = starterKitGuid;
            ctrl.StarterKitDesignInstalled += CtrlStarterKitDesignInstalled;
            ph_skins.Controls.Add(ctrl);

            StarterKitNotInstalled.Visible = false;
            StarterKitInstalled.Visible = true;

        }

        void StarterkitsctrlStarterKitInstalled()
        {
            StarterKitNotInstalled.Visible = false;
            StarterKitInstalled.Visible = false;

            installationCompleted.Visible = true;

        }

        void CtrlStarterKitDesignInstalled()
        {
            StarterKitNotInstalled.Visible = false;
            StarterKitInstalled.Visible = false;

            installationCompleted.Visible = true;
        }

    }
}