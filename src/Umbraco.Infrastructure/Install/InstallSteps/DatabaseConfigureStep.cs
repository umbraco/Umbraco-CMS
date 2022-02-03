using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Core;
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
        private readonly IOptionsMonitor<ConnectionStrings> _connectionStrings;

        public DatabaseConfigureStep(DatabaseBuilder databaseBuilder, IOptionsMonitor<ConnectionStrings> connectionStrings, ILogger<DatabaseConfigureStep> logger)
        {
            _databaseBuilder = databaseBuilder;
            _connectionStrings = connectionStrings;
            _logger = logger;
        }

        public override Task<InstallSetupResult> ExecuteAsync(DatabaseModel database)
        {
            //if the database model is null then we will apply the defaults
            if (database == null)
            {
                database = new DatabaseModel();

                if (IsLocalDbAvailable())
                {
                    database.DatabaseType = DatabaseType.SqlLocalDb;
                }
            }

            if (_databaseBuilder.CanConnect(database.DatabaseType.ToString(), database.ConnectionString, database.Server, database.DatabaseName, database.Login, database.Password, database.IntegratedAuth) == false)
            {
                throw new InstallException("Could not connect to the database");
            }

            ConfigureConnection(database);

            return Task.FromResult<InstallSetupResult>(null);
        }

        private void ConfigureConnection(DatabaseModel database)
        {
            // TODO: PMJ - SQLite/Generic
            if (database.ConnectionString.IsNullOrWhiteSpace() == false)
            {
                _databaseBuilder.ConfigureDatabaseConnection(database.ConnectionString);
            }
            else if (database.DatabaseType == DatabaseType.SqlLocalDb)
            {
                _databaseBuilder.ConfigureSqlLocalDbDatabaseConnection();
            }
            else if (database.IntegratedAuth)
            {
                _databaseBuilder.ConfigureIntegratedSecurityDatabaseConnection(database.Server, database.DatabaseName);
            }
            else
            {
                var password = database.Password.Replace("'", "''");
                password = string.Format("'{0}'", password);

                _databaseBuilder.ConfigureDatabaseConnection(database.Server, database.DatabaseName, database.Login, password, database.DatabaseType.ToString());
            }
        }

        public override object ViewModel
        {
            // TODO: PMJ - SQLite/Generic
            get
            {
                var databases = new List<object>()
                {
                    new { name = "Microsoft SQL Server", id = DatabaseType.SqlServer.ToString() },
                    new { name = "Microsoft SQL Azure", id = DatabaseType.SqlAzure.ToString() },
                    new { name = "Custom connection string", id = DatabaseType.Custom.ToString() },
                };

                if (IsLocalDbAvailable())
                {
                    // Ensure this is always inserted as first when available
                    databases.Insert(0, new { name = "Microsoft SQL Server Express (LocalDB)", id = DatabaseType.SqlLocalDb.ToString() });
                }

                return new
                {
                    databases
                };
            }
        }

        public static bool IsLocalDbAvailable() => new LocalDb().IsAvailable;

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
