using System;
using System.Collections.Generic;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Web.Cache;
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
            //This is synonymous with library.RefreshContent() - but we don't want to use library
            // for anything anymore so welll use the method that it is wrapping. This will just make sure
            // the correct xml structure exists in the xml cache file. This is required by some upgrade scripts
            // that may modify the cmsContentXml table directly.
            DistributedCache.Instance.RefreshAllPageCache();

            // Update configurationStatus
            GlobalSettings.ConfigurationStatus = UmbracoVersion.Current.ToString(3);

            // Update ClientDependency version
            var clientDependencyConfig = new ClientDependencyConfiguration(_applicationContext.ProfilingLogger.Logger);
            var clientDependencyUpdated = clientDependencyConfig.IncreaseVersionNumber();

            var security = new WebSecurity(_httpContext, _applicationContext);
            security.PerformLogin(0);

            ////Clear the auth cookie - this is required so that the login screen is displayed after upgrade and so the 
            //// csrf anti-forgery tokens are created, otherwise there will just be JS errors if the user has an old 
            //// login token from a previous version when we didn't have csrf tokens in place
            //var security = new WebSecurity(new HttpContextWrapper(Context), ApplicationContext.Current);
            //security.ClearCurrentLogin();

            //reports the ended install
            InstallHelper ih = new InstallHelper(UmbracoContext.Current);
            ih.InstallStatus(true, "");

            return null;
        }

        public override bool RequiresExecution(object model)
        {
            return true;
        }
    }
}