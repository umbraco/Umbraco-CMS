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
            var ih = new InstallHelper(UmbracoContext.Current);

            //During a new install we'll log the default user in (which is id = 0).
            // During an upgrade, the user will already need to be logged in in order to run the installer.

            var security = new WebSecurity(_httpContext, _applicationContext);
            //we do this check here because for upgrades the user will already be logged in, for brand new installs,
            // they will not be logged in, however we cannot check the current installation status because it will tell
            // us that it is in 'upgrade' because we already have a database conn configured and a database.
            if (security.IsAuthenticated() == false && GlobalSettings.ConfigurationStatus.IsNullOrWhiteSpace())
            {
                security.PerformLogin(0);
            }

            //This is synonymous with library.RefreshContent() - but we don't want to use library
            // for anything anymore so welll use the method that it is wrapping. This will just make sure
            // the correct xml structure exists in the xml cache file. This is required by some upgrade scripts
            // that may modify the cmsContentXml table directly.
            DistributedCache.Instance.RefreshAllPageCache();

            // Update configurationStatus
            GlobalSettings.ConfigurationStatus = UmbracoVersion.GetSemanticVersion().ToSemanticString();

            // Update ClientDependency version
            var clientDependencyConfig = new ClientDependencyConfiguration(_applicationContext.ProfilingLogger.Logger);
            var clientDependencyUpdated = clientDependencyConfig.IncreaseVersionNumber();
            
            //reports the ended install            
            ih.InstallStatus(true, "");

            return null;
        }

        public override bool RequiresExecution(object model)
        {
            return true;
        }
    }
}