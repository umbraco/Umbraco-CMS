using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Install;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Migrations.PostMigrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Install.InstallSteps
{
    [Obsolete("Will be replace with a new step with the new backoffice")]
    [InstallSetupStep(InstallationType.Upgrade | InstallationType.NewInstall, "DatabaseUpgrade", 12, "")]
    public class DatabaseUpgradeStep : InstallSetupStep<object>
    {
        private readonly DatabaseBuilder _databaseBuilder;
        private readonly IRuntimeState _runtime;
        private readonly ILogger<DatabaseUpgradeStep> _logger;
        private readonly IUmbracoVersion _umbracoVersion;
        private readonly IOptionsMonitor<ConnectionStrings> _connectionStrings;

        public DatabaseUpgradeStep(
            DatabaseBuilder databaseBuilder,
            IRuntimeState runtime,
            ILogger<DatabaseUpgradeStep> logger,
            IUmbracoVersion umbracoVersion,
            IOptionsMonitor<ConnectionStrings> connectionStrings)
        {
            _databaseBuilder = databaseBuilder;
            _runtime = runtime;
            _logger = logger;
            _umbracoVersion = umbracoVersion;
            _connectionStrings = connectionStrings;
        }

        public override Task<InstallSetupResult?> ExecuteAsync(object model)
        {
            InstallTrackingItem[] installSteps = InstallStatusTracker.GetStatus().ToArray();
            InstallTrackingItem previousStep = installSteps.Single(x => x.Name == "DatabaseInstall");
            var upgrade = previousStep.AdditionalData.ContainsKey("upgrade");

            if (upgrade)
            {
                _logger.LogInformation("Running 'Upgrade' service");

                var plan = new UmbracoPlan(_umbracoVersion);
                plan.AddPostMigration<ClearCsrfCookies>(); // needed when running installer (back-office)

                DatabaseBuilder.Result? result = _databaseBuilder.UpgradeSchemaAndData(plan);

                if (result?.Success == false)
                {
                    throw new InstallException("The database failed to upgrade. ERROR: " + result.Message);
                }
            }

            return Task.FromResult((InstallSetupResult?)null);
        }

        public override bool RequiresExecution(object model)
        {
            // If it's properly configured (i.e. the versions match) then no upgrade necessary
            if (_runtime.Level == RuntimeLevel.Run)
            {
                return false;
            }

            // This step relies on the previous one completed - because it has stored some information we need
            InstallTrackingItem[] installSteps = InstallStatusTracker.GetStatus().ToArray();
            if (installSteps.Any(x => x.Name == "DatabaseInstall" && x.AdditionalData.ContainsKey("upgrade")) == false)
            {
                return false;
            }

            if (_connectionStrings.CurrentValue.IsConnectionStringConfigured())
            {
                // A connection string was present, determine whether this is an install/upgrade
                // Return true (upgrade) if there is an installed version, else false (install)
                DatabaseSchemaResult? result = _databaseBuilder.ValidateSchema();
                return result?.DetermineHasInstalledVersion() ?? false;
            }

            // No connection string configured, probably a fresh install
            return false;
        }
    }
}
