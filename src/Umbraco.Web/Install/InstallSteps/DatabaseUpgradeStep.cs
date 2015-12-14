using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep(InstallationType.Upgrade | InstallationType.NewInstall,
        "DatabaseUpgrade", 12, "")]
    internal class DatabaseUpgradeStep : InstallSetupStep<object>
    {
        private readonly ApplicationContext _applicationContext;
        
        public DatabaseUpgradeStep(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public override InstallSetupResult Execute(object model)
        {
            var installSteps = InstallStatusTracker.GetStatus().ToArray();            
            var previousStep = installSteps.Single(x => x.Name == "DatabaseInstall");
            var upgrade = previousStep.AdditionalData.ContainsKey("upgrade");

            if (upgrade)
            {
                LogHelper.Info<DatabaseUpgradeStep>("Running 'Upgrade' service");

                var result = _applicationContext.DatabaseContext.UpgradeSchemaAndData(_applicationContext.Services.MigrationEntryService);

                if (result.Success == false)
                {
                    throw new InstallException("The database failed to upgrade. ERROR: " + result.Message);
                }

                DatabaseInstallStep.HandleConnectionStrings();
            }

            return null;
        }

        public override bool RequiresExecution(object model)
        {
            //if it's properly configured (i.e. the versions match) then no upgrade necessary
            if (_applicationContext.IsConfigured)
            {
                return false;
            }

            var installSteps = InstallStatusTracker.GetStatus().ToArray();
            //this step relies on the previous one completed - because it has stored some information we need
            if (installSteps.Any(x => x.Name == "DatabaseInstall" && x.AdditionalData.ContainsKey("upgrade")) == false)
            {
                return false;
            }
            
            var databaseSettings = ConfigurationManager.ConnectionStrings[GlobalSettings.UmbracoConnectionName];

            if (_applicationContext.DatabaseContext.IsConnectionStringConfigured(databaseSettings))
            {
                //Since a connection string was present we verify whether this is an upgrade or an empty db
                var result = _applicationContext.DatabaseContext.ValidateDatabaseSchema();

                var determinedVersion = result.DetermineInstalledVersion();
                if (determinedVersion.Equals(new Version(0, 0, 0)))
                {
                    //Fresh install
                    return false;
                }

                //Upgrade
                return true;
            }

            //no connection string configured, probably a fresh install
            return false;
        }
    }
}