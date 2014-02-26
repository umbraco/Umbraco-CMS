using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    //internal class DatabaseInstallStep : IInstallStep<object>
    //{
    //    private readonly ApplicationContext _applicationContext;

    //    public DatabaseInstallStep(ApplicationContext applicationContext)
    //    {
    //        _applicationContext = applicationContext;
    //    }

    //    public IDictionary<string, object> Setup(object model)
    //    {
    //        if (CheckConnection(database) == false)
    //        {
    //            throw new InvalidOperationException("Could not connect to the database");
    //        }
    //        ConfigureConnection(database);
    //        return null;
    //    }
    //}

    internal class DatabaseConfigureStep : InstallSetupStep<DatabaseModel>
    {
        private readonly ApplicationContext _applicationContext;

        public DatabaseConfigureStep(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public override IDictionary<string, object> Execute(DatabaseModel database)
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
            get { return "database"; }
        }
    }
}
