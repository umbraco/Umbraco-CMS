using System;
using Umbraco.Core.IO;
using Umbraco.Web.Install;
using Umbraco.Web.UI.Install.Steps.Skinning;

namespace Umbraco.Web.UI.Install.Steps
{
    public partial class StarterKits : StepUserControl
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!global::umbraco.cms.businesslogic.skinning.Skinning.IsStarterKitInstalled())
                ShowStarterKits();
            else
                ShowStarterKitDesigns((Guid)global::umbraco.cms.businesslogic.skinning.Skinning.StarterKitGuid());
        }

       
        private void ShowStarterKits()
        {
            ph_starterKits.Controls.Add(LoadControl(SystemDirectories.Install + "/steps/Skinning/loadStarterKits.ascx"));

            pl_starterKit.Visible = true;
            pl_starterKitDesign.Visible = false;


        }

        private void ShowStarterKitDesigns(Guid starterKitGuid)
        {
            var ctrl = (LoadStarterKitDesigns)LoadControl(SystemDirectories.Install + "/steps/Skinning/loadStarterKitDesigns.ascx");
            ctrl.ID = "StarterKitDesigns";

            ctrl.StarterKitGuid = starterKitGuid;
            ph_starterKitDesigns.Controls.Add(ctrl);

            pl_starterKit.Visible = false;
            pl_starterKitDesign.Visible = true;
        }


    }
}