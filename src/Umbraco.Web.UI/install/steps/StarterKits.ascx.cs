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
            ShowStarterKits();
        }

       
        private void ShowStarterKits()
        {
            ph_starterKits.Controls.Add(LoadControl(SystemDirectories.Install + "/steps/Skinning/loadStarterKits.ascx"));

            pl_starterKit.Visible = true;
            pl_starterKitDesign.Visible = false;


        }
        
    }
}