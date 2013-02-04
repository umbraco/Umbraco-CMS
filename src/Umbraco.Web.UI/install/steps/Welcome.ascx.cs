using System;
using Umbraco.Core;
using umbraco;

namespace Umbraco.Web.UI.Install.Steps
{
    public partial class Welcome : StepUserControl
    {
        protected void Page_Load(object sender, System.EventArgs e)
        {
            var result = ApplicationContext.Current.DatabaseContext.ValidateDatabaseSchema();
            var determinedVersion = result.DetermineInstalledVersion();

            // Display the Umbraco upgrade message if Umbraco is already installed
            if (String.IsNullOrWhiteSpace(GlobalSettings.ConfigurationStatus) == false || determinedVersion.Equals(new Version(0, 0, 0)) == false)
            {
                ph_install.Visible = false;
                ph_upgrade.Visible = true;
            }

            // Check for config!
            if (GlobalSettings.Configured)
            {
                Application.Lock();
                Application["umbracoNeedConfiguration"] = null;
                Application.UnLock();
                Response.Redirect(Request.QueryString["url"] ?? "/", true);
            }
        }

        


    }
}