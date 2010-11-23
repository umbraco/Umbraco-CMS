using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.IO;

namespace umbraco.presentation.umbraco.developer.Packages
{
    public partial class StarterKits : System.Web.UI.Page
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
            ph_starterkits.Controls.Add(new UserControl().LoadControl(SystemDirectories.Install + "/steps/Skinning/loadStarterKits.ascx"));

            StarterKitNotInstalled.Visible = true;
            StarterKitInstalled.Visible = false;

        }

        public void showSkins(Guid starterKitGuid)
        {
            install.steps.Skinning.loadStarterKitDesigns ctrl = (install.steps.Skinning.loadStarterKitDesigns)new UserControl().LoadControl(SystemDirectories.Install + "/steps/Skinning/loadStarterKitDesigns.ascx");
            ctrl.ID = "StarterKitDesigns";

            ctrl.StarterKitGuid = starterKitGuid;

            ph_skins.Controls.Add(ctrl);

            StarterKitNotInstalled.Visible = false;
            StarterKitInstalled.Visible = true;

        }
    }
}