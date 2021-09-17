using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Install;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Install.InstallSteps
{
    [InstallSetupStep(InstallationType.NewInstall | InstallationType.Upgrade,
        "DatabaseInstall", 11, "")]
    public class DatabaseInstallStep : InstallSetupStep<object>
    {
        private readonly DatabaseBuilder _databaseBuilder;
        private readonly IRuntimeState _runtime;
        private readonly IOptionsMonitor<GlobalSettings> _globalSettings;

        public DatabaseInstallStep(DatabaseBuilder databaseBuilder, IRuntimeState runtime, IOptionsMonitor<GlobalSettings> globalSettings)
        {
            _databaseBuilder = databaseBuilder;
            _runtime = runtime;
            _globalSettings = globalSettings;
        }

        public override Task<InstallSetupResult> ExecuteAsync(object model)
        {
            if (_runtime.Level == RuntimeLevel.Run)
                throw new Exception("Umbraco is already configured!");

            if (_globalSettings.CurrentValue.InstallMissingDatabase)
            {
                _databaseBuilder.CreateDatabase();
            }

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
