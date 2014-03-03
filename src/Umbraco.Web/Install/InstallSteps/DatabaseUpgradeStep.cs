using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep("DatabaseUpgrade", "", "Upgrading your database to the latest version")]
    internal class DatabaseUpgradeStep : InstallSetupStep<object>
    {
        private readonly ApplicationContext _applicationContext;

        public DatabaseUpgradeStep(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public override IDictionary<string, object> Execute(object model)
        {
            var installSteps = InstallStatusTracker.GetStatus();
            //this step relies on the preious one completed - because it has stored some information we need
            if (installSteps.Any(x => x.Key == "DatabaseConfigure") == false)
            {
                throw new InvalidOperationException("Could not find previous step: DatabaseConfigure of the installation, package install cannot continue");
            }
            var previousStep = installSteps["DatabaseConfigure"];
            var upgrade = previousStep.AdditionalData.ContainsKey("upgrade");

            if (upgrade)
            {
                LogHelper.Info<DatabaseUpgradeStep>("Running 'Upgrade' service");

                var result = _applicationContext.DatabaseContext.UpgradeSchemaAndData();

                DatabaseInstallStep.HandleConnectionStrings();
            }

            return null;
        }

    }
}