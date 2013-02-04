using System;
using Umbraco.Core.Configuration;

namespace Umbraco.Web.UI.Install.Steps
{
    public partial class TheEnd : StepUserControl
    {
        protected void Page_Load(object sender, System.EventArgs e)
        {
            // Update configurationStatus
            try
            {

                GlobalSettings.ConfigurationStatus = UmbracoVersion.Current.ToString(3);
                Application["umbracoNeedConfiguration"] = false;
            }
            catch (Exception)
            {
                //errorLiteral.Text = ex.ToString();
            }

            // Update ClientDependency version
            var clientDependencyConfig = new ClientDependencyConfiguration();
            var clientDependencyUpdated = clientDependencyConfig.IncreaseVersionNumber();

            if (!global::umbraco.cms.businesslogic.skinning.Skinning.IsStarterKitInstalled())
                customizeSite.Visible = false;

        }

    }
}