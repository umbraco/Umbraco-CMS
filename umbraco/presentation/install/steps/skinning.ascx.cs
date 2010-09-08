using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.IO;

namespace umbraco.presentation.install.steps
{
    public partial class skinning : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!cms.businesslogic.skinning.Skinning.IsStarterKitInstalled())
                showStarterKits();
            else
                showStarterKitDesigns((Guid)cms.businesslogic.skinning.Skinning.StarterKitGuid());
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

           

        }
        private void showStarterKits()
        {
            ph_starterKits.Controls.Add(new UserControl().LoadControl(SystemDirectories.Install + "/steps/Skinning/loadStarterKits.ascx"));

            pl_starterKit.Visible = true;
            pl_starterKitDesign.Visible = false;

           
        }

        public void showStarterKitDesigns(Guid starterKitGuid)
        {
            Skinning.loadStarterKitDesigns ctrl = (Skinning.loadStarterKitDesigns)new UserControl().LoadControl(SystemDirectories.Install + "/steps/Skinning/loadStarterKitDesigns.ascx");
            ctrl.ID = "StarterKitDesigns";

            ctrl.StarterKitGuid = starterKitGuid;

            ph_starterKitDesigns.Controls.Add(ctrl);

            pl_starterKit.Visible = false;
            pl_starterKitDesign.Visible = true;

        }

        public void showCustomizeSkin()
        {
            Response.Redirect(GlobalSettings.Path + "/canvas.aspx?redir=" + this.ResolveUrl("~/") + "&skinning=true");
        }
    }
}