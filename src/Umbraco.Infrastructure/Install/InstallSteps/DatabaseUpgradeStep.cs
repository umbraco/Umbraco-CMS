﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.IO;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Migrations.Upgrade;
using Umbraco.Web.Install.Models;
using Umbraco.Web.Migrations.PostMigrations;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep(InstallationType.Upgrade | InstallationType.NewInstall,
        "DatabaseUpgrade", 12, "")]
    public class DatabaseUpgradeStep : InstallSetupStep<object>
    {
        private readonly DatabaseBuilder _databaseBuilder;
        private readonly IRuntimeState _runtime;
        private readonly ILogger<DatabaseUpgradeStep> _logger;
        private readonly IUmbracoVersion _umbracoVersion;
        private readonly ConnectionStrings _connectionStrings;

        public DatabaseUpgradeStep(
            DatabaseBuilder databaseBuilder,
            IRuntimeState runtime,
            ILogger<DatabaseUpgradeStep> logger,
            IUmbracoVersion umbracoVersion,
            IOptions<ConnectionStrings> connectionStrings)
        {
            _databaseBuilder = databaseBuilder;
            _runtime = runtime;
            _logger = logger;
            _umbracoVersion = umbracoVersion;
            _connectionStrings = connectionStrings.Value ?? throw new ArgumentNullException(nameof(connectionStrings));
        }

        public override Task<InstallSetupResult> ExecuteAsync(object model)
        {
            var installSteps = InstallStatusTracker.GetStatus().ToArray();
            var previousStep = installSteps.Single(x => x.Name == "DatabaseInstall");
            var upgrade = previousStep.AdditionalData.ContainsKey("upgrade");

            if (upgrade)
            {
                _logger.LogInformation("Running 'Upgrade' service");

                var plan = new UmbracoPlan(_umbracoVersion);
                plan.AddPostMigration<ClearCsrfCookies>(); // needed when running installer (back-office)

                var result = _databaseBuilder.UpgradeSchemaAndData(plan);

                if (result.Success == false)
                {
                    throw new InstallException("The database failed to upgrade. ERROR: " + result.Message);
                }
            }

            return Task.FromResult<InstallSetupResult>(null);
        }

        public override bool RequiresExecution(object model)
        {
            //if it's properly configured (i.e. the versions match) then no upgrade necessary
            if (_runtime.Level == RuntimeLevel.Run)
                return false;

            var installSteps = InstallStatusTracker.GetStatus().ToArray();
            //this step relies on the previous one completed - because it has stored some information we need
            if (installSteps.Any(x => x.Name == "DatabaseInstall" && x.AdditionalData.ContainsKey("upgrade")) == false)
            {
                return false;
            }

            var databaseSettings = _connectionStrings.UmbracoConnectionString;

            if (databaseSettings.IsConnectionStringConfigured())
            {
                // a connection string was present, determine whether this is an install/upgrade
                // return true (upgrade) if there is an installed version, else false (install)
                var result = _databaseBuilder.ValidateSchema();
                return result.DetermineHasInstalledVersion();
            }

            //no connection string configured, probably a fresh install
            return false;
        }
    }
}
