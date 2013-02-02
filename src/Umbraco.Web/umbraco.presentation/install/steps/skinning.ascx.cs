using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.IO;

namespace umbraco.presentation.install.steps
{
    [Obsolete("This class is no longer used and will be removed from the codebase in the future. The UserControl that supercedes this is Umbraco.Web.UI.Install.Steps.StarterKits")]
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
            //Response.Redirect(GlobalSettings.Path + "/canvas.aspx?redir=" + this.ResolveUrl("~/") + "&umbSkinning=true&umbSkinningConfigurator=true");

            _default p = (_default)this.Page;
            p.GotoNextStep(helper.Request("installStep"));
        }

        protected void gotoNextStep(object sender, EventArgs e)
        {
            Helper.RedirectToNextStep(this.Page);
        }

        /// <summary>
        /// udp control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected UpdatePanel udp;

        /// <summary>
        /// pl_starterKit control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder pl_starterKit;

        /// <summary>
        /// ph_starterKits control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder ph_starterKits;

        /// <summary>
        /// pl_starterKitDesign control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder pl_starterKitDesign;

        /// <summary>
        /// ph_starterKitDesigns control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder ph_starterKitDesigns;
    }
}