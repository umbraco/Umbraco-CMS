using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Install;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Install.InstallSteps
{
    [InstallSetupStep(InstallationType.NewInstall,
        "DatabaseConfigure", "database", 10, "Setting up a database, so Umbraco has a place to store your website",
        PerformsAppRestart = true)]
    public class DatabaseConfigureStep : InstallSetupStep<DatabaseModel>
    {
        private readonly DatabaseBuilder _databaseBuilder;
        private readonly ILogger<DatabaseConfigureStep> _logger;
        private readonly IEnumerable<IDatabaseProviderMetadata> _databaseProviderMetadata;
        private readonly IOptionsMonitor<ConnectionStrings> _connectionStrings;

        public DatabaseConfigureStep(
            DatabaseBuilder databaseBuilder,
            IOptionsMonitor<ConnectionStrings> connectionStrings,
            ILogger<DatabaseConfigureStep> logger,
            IEnumerable<IDatabaseProviderMetadata> databaseProviderMetadata)
        {
            _databaseBuilder = databaseBuilder;
            _connectionStrings = connectionStrings;
            _logger = logger;
            _databaseProviderMetadata = databaseProviderMetadata;
        }

        public override Task<InstallSetupResult?> ExecuteAsync(DatabaseModel databaseSettings)
        {
            if (!_databaseBuilder.ConfigureDatabaseConnection(databaseSettings, isTrialRun: false))
            {
                throw new InstallException("Could not connect to the database");
            }

            return Task.FromResult<InstallSetupResult?>(null);
        }

        public override object ViewModel
        {
            get
            {
                var options = _databaseProviderMetadata
                    .Where(x => x.IsAvailable)
                    .OrderBy(x => x.SortOrder)
                    .ToList();

                return new
                {
                    databases = options
                };
            }
        }

        public override string View => ShouldDisplayView() ? base.View : "";

        public override bool RequiresExecution(DatabaseModel model) => ShouldDisplayView();

        private bool ShouldDisplayView()
        {
            //If the connection string is already present in web.config we don't need to show the settings page and we jump to installing/upgrading.
            var databaseSettings = _connectionStrings.Get(Core.Constants.System.UmbracoConnectionName);

            if (databaseSettings.IsConnectionStringConfigured())
            {
                try
                {
                    //Since a connection string was present we verify the db can connect and query
                    _ = _databaseBuilder.ValidateSchema();

                    return false;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred, reconfiguring...");
                    //something went wrong, could not connect so probably need to reconfigure
                    return true;
                }
            }

            return true;
        }
    }
}
