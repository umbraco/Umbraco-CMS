using System;
using System.Collections.Generic;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Web.Install.Models;
using Umbraco.Web.Security;
using GlobalSettings = umbraco.GlobalSettings;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep(InstallationType.NewInstall | InstallationType.Upgrade,
        "UmbracoVersion", 50, "Installation is complete!, get ready to be redirected to your new CMS.",
        PerformsAppRestart = true)]
    internal class SetUmbracoVersionStep : InstallSetupStep<object>
    {
        private readonly ApplicationContext _applicationContext;
        private readonly HttpContextBase _httpContext;

        public SetUmbracoVersionStep(ApplicationContext applicationContext, HttpContextBase httpContext)
        {
            _applicationContext = applicationContext;
            _httpContext = httpContext;
        }

        public override InstallSetupResult Execute(object model)
        {
            // Update configurationStatus
            GlobalSettings.ConfigurationStatus = UmbracoVersion.Current.ToString(3);

            // Update ClientDependency version
            var clientDependencyConfig = new ClientDependencyConfiguration();
            var clientDependencyUpdated = clientDependencyConfig.IncreaseVersionNumber();

            var security = new WebSecurity(_httpContext, _applicationContext);
            security.PerformLogin(0);

            ////Clear the auth cookie - this is required so that the login screen is displayed after upgrade and so the 
            //// csrf anti-forgery tokens are created, otherwise there will just be JS errors if the user has an old 
            //// login token from a previous version when we didn't have csrf tokens in place
            //var security = new WebSecurity(new HttpContextWrapper(Context), ApplicationContext.Current);
            //security.ClearCurrentLogin();
            return null;
        }

        public override bool RequiresExecution(object model)
        {
            return true;
        }
    }
}