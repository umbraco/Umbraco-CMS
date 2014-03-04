using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep("DatabaseConfigure", "database", 3, "Configuring your database connection")]
    internal class DatabaseConfigureStep : InstallSetupStep<DatabaseModel>
    {
        private readonly ApplicationContext _applicationContext;

        public DatabaseConfigureStep(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public override InstallSetupResult Execute(DatabaseModel database)
        {
            if (CheckConnection(database) == false)
            {
                throw new InvalidOperationException("Could not connect to the database");
            }
            ConfigureConnection(database);
            return null;
        }

        private bool CheckConnection(DatabaseModel database)
        {
            string connectionString;
            var dbContext = _applicationContext.DatabaseContext;
            if (database.ConnectionString.IsNullOrWhiteSpace() == false)
            {
                connectionString = database.ConnectionString;
            }
            else if (database.DatabaseType == DatabaseType.SqlCe)
            {
                //we do not test this connection
                return true;
                //connectionString = dbContext.GetEmbeddedDatabaseConnectionString();
            }
            else if (database.IntegratedAuth)
            {
                connectionString = dbContext.GetIntegratedSecurityDatabaseConnectionString(
                    database.Server, database.DatabaseName);
            }
            else
            {
                string providerName;
                connectionString = dbContext.GetDatabaseConnectionString(
                    database.Server, database.DatabaseName, database.Login, database.Password,
                    database.DatabaseType.ToString(),
                    out providerName);
            }

            return SqlExtensions.IsConnectionAvailable(connectionString);
        }

        private void ConfigureConnection(DatabaseModel database)
        {
            var dbContext = _applicationContext.DatabaseContext;
            if (database.ConnectionString.IsNullOrWhiteSpace() == false)
            {
                dbContext.ConfigureDatabaseConnection(database.ConnectionString);
            }
            else if (database.DatabaseType == DatabaseType.SqlCe)
            {
                dbContext.ConfigureEmbeddedDatabaseConnection();
            }
            else if (database.IntegratedAuth)
            {
                dbContext.ConfigureIntegratedSecurityDatabaseConnection(
                    database.Server, database.DatabaseName);
            }
            else
            {
                dbContext.ConfigureDatabaseConnection(
                    database.Server, database.DatabaseName, database.Login, database.Password,
                    database.DatabaseType.ToString());
            }
        }

        public override string View
        {
            get { return ShouldDisplayView() ? base.View : ""; }
        }

        public override bool RequiresExecution()
        {
            //TODO: We need to determine if we should run this based on whether the conn string is configured already
            return true;
        }

        private bool ShouldDisplayView()
        {
            //If the connection string is already present in web.config we don't need to show the settings page and we jump to installing/upgrading.
            var databaseSettings = ConfigurationManager.ConnectionStrings[GlobalSettings.UmbracoConnectionName];

            var dbIsSqlCe = false;
            if (databaseSettings != null && databaseSettings.ProviderName != null)
                dbIsSqlCe = databaseSettings.ProviderName == "System.Data.SqlServerCe.4.0";
            var sqlCeDatabaseExists = false;
            if (dbIsSqlCe)
            {
                var datasource = databaseSettings.ConnectionString.Replace("|DataDirectory|", AppDomain.CurrentDomain.GetData("DataDirectory").ToString());
                var filePath = datasource.Replace("Data Source=", string.Empty);
                sqlCeDatabaseExists = File.Exists(filePath);
            }

            // Either the connection details are not fully specified or it's a SQL CE database that doesn't exist yet
            if (databaseSettings == null
                || string.IsNullOrWhiteSpace(databaseSettings.ConnectionString) || string.IsNullOrWhiteSpace(databaseSettings.ProviderName)
                || (dbIsSqlCe && sqlCeDatabaseExists == false))
            {
                return true;
            }
            else
            {
                //Since a connection string was present we verify whether this is an upgrade or an empty db
                var result = ApplicationContext.Current.DatabaseContext.ValidateDatabaseSchema();
                var determinedVersion = result.DetermineInstalledVersion();
                if (determinedVersion.Equals(new Version(0, 0, 0)))
                {
                    //Fresh install
                    return false;
                }
                else
                {
                    //Upgrade
                    return false;
                }
            }
        }
    }
}
