using System;
using System.Configuration;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations.Install;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep(InstallationType.NewInstall,
        "DatabaseConfigure", "database", 10, "Setting up a database, so Umbraco has a place to store your website",
        PerformsAppRestart = true)]
    internal class DatabaseConfigureStep : InstallSetupStep<DatabaseModel>
    {
        private readonly DatabaseBuilder _databaseBuilder;
        private readonly ILogger _logger;

        public DatabaseConfigureStep(DatabaseBuilder databaseBuilder)
        {
            _databaseBuilder = databaseBuilder;
        }

        public override Task<InstallSetupResult> ExecuteAsync(DatabaseModel database)
        {
            //if the database model is null then we will apply the defaults
            if (database == null)
            {
                database = new DatabaseModel();
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
            if (database.ConnectionString.IsNullOrWhiteSpace() == false)
            {
                _databaseBuilder.ConfigureDatabaseConnection(database.ConnectionString);
            }
            else if (database.DatabaseType == DatabaseType.SqlCe)
            {
                _databaseBuilder.ConfigureEmbeddedDatabaseConnection();
            }
            else if (database.IntegratedAuth)
            {
                _databaseBuilder.ConfigureIntegratedSecurityDatabaseConnection(database.Server, database.DatabaseName);
            }
            else
            {
                var password = database.Password.Replace("'", "''");
                password = string.Format("'{0}'", password);

                _databaseBuilder.ConfigureDatabaseConnection(
                    database.Server, database.DatabaseName, database.Login, password,
                    database.DatabaseType.ToString());
            }
        }

        public override string View => ShouldDisplayView() ? base.View : "";

        public override bool RequiresExecution(DatabaseModel model)
        {
            return ShouldDisplayView();
        }

        private bool ShouldDisplayView()
        {
            //If the connection string is already present in web.config we don't need to show the settings page and we jump to installing/upgrading.
            var databaseSettings = ConfigurationManager.ConnectionStrings[Constants.System.UmbracoConnectionName];

            if (_databaseBuilder.IsConnectionStringConfigured(databaseSettings))
            {
                try
                {
                    //Since a connection string was present we verify the db can connect and query
                    _ = _databaseBuilder.ValidateSchema();
                    return false;
                }
                catch (Exception ex)
                {
                    _logger.Error<DatabaseConfigureStep>(ex, "An error occurred, reconfiguring...");
                    //something went wrong, could not connect so probably need to reconfigure
                    return true;
                }
            }

            return true;
        }
    }
}
