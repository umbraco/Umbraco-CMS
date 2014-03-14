using System;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Web.Security;

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
            }
            catch (Exception ex)
            {
                LogHelper.Error<TheEnd>("An error occurred updating the config status", ex);
            }

            // Update ClientDependency version
            var clientDependencyConfig = new ClientDependencyConfiguration();
            var clientDependencyUpdated = clientDependencyConfig.IncreaseVersionNumber();
            
            //Clear the auth cookie - this is required so that the login screen is displayed after upgrade and so the 
            // csrf anti-forgery tokens are created, otherwise there will just be JS errors if the user has an old 
            // login token from a previous version when we didn't have csrf tokens in place
            var security = new WebSecurity(new HttpContextWrapper(Context), ApplicationContext.Current);
            security.ClearCurrentLogin();
        }

    }
}