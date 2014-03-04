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
    [InstallSetupStep(InstallationType.Upgrade,
        "DatabaseUpgrade", 12, "Upgrading your database to the latest version")]
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
            //this step relies on the preious one completed - because it has stored some information we need
            if (installSteps.Any(x => x.Name == "DatabaseConfigure") == false)
            {
                throw new InvalidOperationException("Could not find previous step: DatabaseConfigure of the installation, package install cannot continue");
            }
            var previousStep = installSteps.Single(x => x.Name == "DatabaseConfigure");
            var upgrade = previousStep.AdditionalData.ContainsKey("upgrade");

            if (upgrade)
            {
                LogHelper.Info<DatabaseUpgradeStep>("Running 'Upgrade' service");

                var result = _applicationContext.DatabaseContext.UpgradeSchemaAndData();

                DatabaseInstallStep.HandleConnectionStrings();
            }

            return null;
        }

        public override bool RequiresExecution()
        {
            //if it's properly configured (i.e. the versions match) then no upgrade necessary
            if (_applicationContext.IsConfigured)
            {
                return false;
            }
            
            var databaseSettings = ConfigurationManager.ConnectionStrings[GlobalSettings.UmbracoConnectionName];

            if (_applicationContext.DatabaseContext.IsConnectionStringConfigured(databaseSettings))
            {
                try
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
                catch (Exception)
                {
                    //something went wrong, could not connect so probably need to reconfigure
                    return false;
                }
            }

            //no connection string configured, probably a fresh install
            return false;
        }
    }
}