using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BasePages;
using umbraco.IO;

namespace umbraco.presentation.umbraco.developer.Packages
{
    public partial class StarterKits : UmbracoEnsuredPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!cms.businesslogic.skinning.Skinning.IsStarterKitInstalled())
                showStarterKits();
            else
                showSkins((Guid)cms.businesslogic.skinning.Skinning.StarterKitGuid());
        }

        private void showStarterKits()
        {
            install.steps.Skinning.loadStarterKits starterkitsctrl = 
                (install.steps.Skinning.loadStarterKits)new UserControl().LoadControl(SystemDirectories.Install + "/steps/Skinning/loadStarterKits.ascx");
            starterkitsctrl.StarterKitInstalled+=new install.steps.Skinning.StarterKitInstalledEventHandler(starterkitsctrl_StarterKitInstalled);
            
            ph_starterkits.Controls.Add(starterkitsctrl);

            

            StarterKitNotInstalled.Visible = true;
            StarterKitInstalled.Visible = false;

        }

        

        public void showSkins(Guid starterKitGuid)
        {

            install.steps.Skinning.loadStarterKitDesigns ctrl = (install.steps.Skinning.loadStarterKitDesigns)new UserControl().LoadControl(SystemDirectories.Install + "/steps/Skinning/loadStarterKitDesigns.ascx");
            ctrl.ID = "StarterKitDesigns";

            ctrl.StarterKitGuid = starterKitGuid;
            ctrl.StarterKitDesignInstalled += new install.steps.Skinning.StarterKitDesignInstalledEventHandler(ctrl_StarterKitDesignInstalled);
            ph_skins.Controls.Add(ctrl);

            StarterKitNotInstalled.Visible = false;
            StarterKitInstalled.Visible = true;

        }

        void starterkitsctrl_StarterKitInstalled()
        {
            StarterKitNotInstalled.Visible = false;
            StarterKitInstalled.Visible = false;

            installationCompleted.Visible = true;
           
        }

        void ctrl_StarterKitDesignInstalled()
        {
            StarterKitNotInstalled.Visible = false;
            StarterKitInstalled.Visible = false;

            installationCompleted.Visible = true;
        }
    }
}