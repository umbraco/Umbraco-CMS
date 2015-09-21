using System;
using System.Collections.Generic;
using System.Configuration;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep(InstallationType.NewInstall | InstallationType.Upgrade,
        "DatabaseInstall", 11, "")]
    internal class DatabaseInstallStep : InstallSetupStep<object>
    {
        private readonly ApplicationContext _applicationContext;

        public DatabaseInstallStep(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public override InstallSetupResult Execute(object model)
        {
            var result = _applicationContext.DatabaseContext.CreateDatabaseSchemaAndData(_applicationContext);

            if (result.Success == false)
            {
                throw new InstallException("The database failed to install. ERROR: " + result.Message);
            }
            
            if (result.RequiresUpgrade == false)
            {
                HandleConnectionStrings();
                return null;
            }
            else
            {
                //upgrade is required so set the flag for the next step

                return new InstallSetupResult(new Dictionary<string, object>
                {
                    {"upgrade", true}
                });
            }
            
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

        public override bool RequiresExecution(object model)
        {
            return true;
        }
    }
}