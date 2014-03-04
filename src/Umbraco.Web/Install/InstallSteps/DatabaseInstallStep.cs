using System;
using System.Collections.Generic;
using System.Configuration;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep("DatabaseInstall", 11, "Installing database tables and default system data")]
    internal class DatabaseInstallStep : InstallSetupStep<object>
    {
        private readonly ApplicationContext _applicationContext;

        public DatabaseInstallStep(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public override InstallSetupResult Execute(object model)
        {
            var result = _applicationContext.DatabaseContext.CreateDatabaseSchemaAndData();
            if (result.RequiresUpgrade == false)
            {
                HandleConnectionStrings();
                return new InstallSetupResult(new Dictionary<string, object>
                {
                    {"upgrade", true}
                });
            }   
            return null;
        }

        internal static void HandleConnectionStrings()
        {
            // Remove legacy umbracoDbDsn configuration setting if it exists and connectionstring also exists
            if (ConfigurationManager.ConnectionStrings[GlobalSettings.UmbracoConnectionName] != null)
            {
                GlobalSettings.RemoveSetting(GlobalSettings.UmbracoConnectionName);
            }
            else
            {
                var ex = new ArgumentNullException(string.Format("ConfigurationManager.ConnectionStrings[{0}]", GlobalSettings.UmbracoConnectionName), "Install / upgrade did not complete successfully, umbracoDbDSN was not set in the connectionStrings section");
                LogHelper.Error<DatabaseInstallStep>("", ex);
                throw ex;
            }
        }

        public override bool RequiresExecution()
        {
            return true;
        }
    }
}