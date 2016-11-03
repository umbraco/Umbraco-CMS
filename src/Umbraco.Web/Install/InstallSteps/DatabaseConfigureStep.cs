using System;
using System.Configuration;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep(InstallationType.NewInstall,
        "DatabaseConfigure", "database", 10, "Setting up a database, so Umbraco has a place to store your website",
        PerformsAppRestart = true)]
    internal class DatabaseConfigureStep : InstallSetupStep<DatabaseModel>
    {
        private readonly DatabaseContext _databaseContext;

        public DatabaseConfigureStep(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public override InstallSetupResult Execute(DatabaseModel database)
        {
            //if the database model is null then we will apply the defaults
            if (database == null)
            {
                database = new DatabaseModel();
            }

            var dbHelper = new DatabaseHelper();

            if (dbHelper.CheckConnection(database) == false)
            {
                throw new InstallException("Could not connect to the database");
            }
            ConfigureConnection(database);
            return null;
        }

        private void ConfigureConnection(DatabaseModel database)
        {
            if (database.ConnectionString.IsNullOrWhiteSpace() == false)
            {
                _databaseContext.ConfigureDatabaseConnection(database.ConnectionString);
            }
            else if (database.DatabaseType == DatabaseType.SqlCe)
            {
                _databaseContext.ConfigureEmbeddedDatabaseConnection();
            }
            else if (database.IntegratedAuth)
            {
                _databaseContext.ConfigureIntegratedSecurityDatabaseConnection(
                    database.Server, database.DatabaseName);
            }
            else
            {
                var password = database.Password.Replace("&", "&amp;").Replace(">", "&gt;").Replace("<", "&lt;").Replace("\"", "&quot;").Replace("'", "''");
                password = string.Format("'{0}'", password);

                _databaseContext.ConfigureDatabaseConnection(
                    database.Server, database.DatabaseName, database.Login, password,
                    database.DatabaseType.ToString());
            }
        }

        public override string View
        {
            get { return ShouldDisplayView() ? base.View : ""; }
        }

        public override bool RequiresExecution(DatabaseModel model)
        {
            return ShouldDisplayView();
        }

        private bool ShouldDisplayView()
        {
            //If the connection string is already present in web.config we don't need to show the settings page and we jump to installing/upgrading.
            var databaseSettings = ConfigurationManager.ConnectionStrings[GlobalSettings.UmbracoConnectionName];

            if (_databaseContext.IsConnectionStringConfigured(databaseSettings))
            {
                try
                {
                    //Since a connection string was present we verify the db can connect and query
                    var result = _databaseContext.ValidateDatabaseSchema();
                    result.DetermineInstalledVersion();
                    return false;
                }
                catch (Exception)
                {
                    //something went wrong, could not connect so probably need to reconfigure
                    return true;
                }
            }

            return true;
        }
    }
}
