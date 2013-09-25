using System;
using System.Linq;
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
            ShowStarterKits();
        }

        private void ShowStarterKits()
        {
            if (Directory.Exists(Server.MapPath(SystemDirectories.Install)) == false)
            {
                InstallationDirectoryNotAvailable.Visible = true;
                StarterKitNotInstalled.Visible = false;
                StarterKitInstalled.Visible = false;
            
                return;
            }


            var starterkitsctrl = (LoadStarterKits)LoadControl(SystemDirectories.Install + "/steps/Skinning/loadStarterKits.ascx");
            starterkitsctrl.StarterKitInstalled += StarterkitsctrlStarterKitInstalled;

            ph_starterkits.Controls.Add(starterkitsctrl);

            StarterKitNotInstalled.Visible = true;
            StarterKitInstalled.Visible = false;

        }
        
        void StarterkitsctrlStarterKitInstalled()
        {
            StarterKitNotInstalled.Visible = false;
            StarterKitInstalled.Visible = false;

            installationCompleted.Visible = true;

        }

    }
}