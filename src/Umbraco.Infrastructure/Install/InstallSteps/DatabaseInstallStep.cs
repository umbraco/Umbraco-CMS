using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations.Install;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep(InstallationType.NewInstall | InstallationType.Upgrade,
        "DatabaseInstall", 11, "")]
    [UmbracoVolatile]
    public class DatabaseInstallStep : InstallSetupStep<object>
    {
        private readonly DatabaseBuilder _databaseBuilder;
        private readonly IRuntimeState _runtime;

        public DatabaseInstallStep(DatabaseBuilder databaseBuilder, IRuntimeState runtime)
        {
            _databaseBuilder = databaseBuilder;
            _runtime = runtime;
        }

        public override Task<InstallSetupResult> ExecuteAsync(object model)
        {
            if (_runtime.Level == RuntimeLevel.Run)
                throw new Exception("Umbraco is already configured!");

            var result = _databaseBuilder.CreateSchemaAndData();

            if (result.Success == false)
            {
                throw new InstallException("The database failed to install. ERROR: " + result.Message);
            }

            if (result.RequiresUpgrade == false)
            {
                return Task.FromResult<InstallSetupResult>(null);
            }

            //upgrade is required so set the flag for the next step
            return Task.FromResult(new InstallSetupResult(new Dictionary<string, object>
            {
                {"upgrade", true}
            }));
        }

        public override bool RequiresExecution(object model)
        {
            return true;
        }
    }
}
