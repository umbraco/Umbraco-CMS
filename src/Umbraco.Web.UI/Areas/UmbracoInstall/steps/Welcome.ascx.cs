using System;
using Umbraco.Core;
using umbraco;

namespace Umbraco.Web.UI.Install.Steps
{
    public partial class Welcome : StepUserControl
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (!IsPostBack)
            {
                //clear the plugin cache when installation starts (just a safety check)
                PluginManager.Current.ClearPluginCache();    
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {            
            // Check for config! SD: I've moved this config check above the other stuff since there's no point in doing all that processing if we're
            // just going to redirect if this setting is true.
            if (GlobalSettings.Configured)
            {
                Response.Redirect(Request.QueryString["url"] ?? "/", true);
            }

            var result = ApplicationContext.Current.DatabaseContext.ValidateDatabaseSchema();
            var determinedVersion = result.DetermineInstalledVersion();

            // Display the Umbraco upgrade message if Umbraco is already installed
            if (string.IsNullOrWhiteSpace(GlobalSettings.ConfigurationStatus) == false || determinedVersion.Equals(new Version(0, 0, 0)) == false)
            {
                ph_install.Visible = false;
                ph_upgrade.Visible = true;
            }
            
        }

        


    }
}