using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install
{
    // that class was originally created by Per - tests the db connection for install
    // fixed by Shannon to not-ignore the provider
    // fixed by Stephan as part of the v8 persistence cleanup, now using provider names + SqlCe exception

    internal class DatabaseHelper
    {
        internal bool CheckConnection(DatabaseModel model)
        {
            // we do not test SqlCE connection
            if (model.DatabaseType == DatabaseType.SqlCe)
                return true;

            string providerName;
            string connectionString;

            if (string.IsNullOrWhiteSpace(model.ConnectionString) == false)
            {
                providerName = DbConnectionExtensions.DetectProviderNameFromConnectionString(model.ConnectionString);
                connectionString = model.ConnectionString;
            }
            else if (model.IntegratedAuth)
            {
                // has to be Sql Server
                providerName = Constants.DbProviderNames.SqlServer;
                connectionString = DatabaseContext.GetIntegratedSecurityDatabaseConnectionString(model.Server, model.DatabaseName);
            }
            else
            {
                connectionString = DatabaseContext.GetDatabaseConnectionString(
                    model.Server, model.DatabaseName, model.Login, model.Password,
                    model.DatabaseType.ToString(), out providerName);
            }

            return DbConnectionExtensions.IsConnectionAvailable(connectionString, providerName);
        }
    }
}
