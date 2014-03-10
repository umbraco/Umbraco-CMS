using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install
{
    internal class DatabaseHelper
    {
        internal bool CheckConnection(DatabaseModel database, ApplicationContext applicationContext)
        {
            string connectionString;
            var dbContext = applicationContext.DatabaseContext;

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
    }
}
