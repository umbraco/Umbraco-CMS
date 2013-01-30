using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core.Configuration;
using umbraco.presentation.install;
using Umbraco.Web.Install.UpgradeScripts;

namespace Umbraco.Web.UI.Install.Steps
{
    public partial class UpgradeScripts : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void RunScripts(object sender, EventArgs e)
        {
            //run the scripts and then go to the next step
            UpgradeScriptManager.ExecuteScriptsForVersion(GlobalSettings.GetConfigurationVersion());
            
            Helper.RedirectToNextStep(Page);
        }
    }
}