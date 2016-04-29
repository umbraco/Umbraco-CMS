using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install
{
    internal class DatabaseHelper
    {
        internal bool CheckConnection(DatabaseContext context, DatabaseModel model)
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
                connectionString = context.GetIntegratedSecurityDatabaseConnectionString(model.Server, model.DatabaseName);
            }
            else
            {
                connectionString = context.GetDatabaseConnectionString(
                    model.Server, model.DatabaseName, model.Login, model.Password,
                    model.DatabaseType.ToString(), out providerName);
            }

            return DbConnectionExtensions.IsConnectionAvailable(connectionString, providerName);
        }
    }
}
