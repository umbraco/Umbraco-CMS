using System;
using System.Collections.Generic;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Web.Cache;
using Umbraco.Web.Install.Models;
using Umbraco.Web.Security;


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

            // Some upgrade scripts "may modify the database (cmsContentXml...) tables directly" - not sure
            // that is still true but the idea is that after an upgrade we want to reset the local facade, on
            // all LB nodes of course, so we need to use the distributed cache, and refresh everything.
            DistributedCache.Instance.RefreshAllFacade();

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