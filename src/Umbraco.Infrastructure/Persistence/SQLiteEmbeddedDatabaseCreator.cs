using System;
using System.Collections.Generic;
using System.Text;

namespace Umbraco.Cms.Infrastructure.Persistence
{
    public class SQLiteEmbeddedDatabaseCreator : IEmbeddedDatabaseCreator
    {
        public string ProviderName => Cms.Core.Constants.DatabaseProviders.SQLite;

        public string ConnectionString { get; set; } = "Data Source=C:/Application.db;";

        public void Create()
        {
            // Do we need to do anything as SQLite will create file if not exist it seems

            // Parse Connection String
            var connection = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(ConnectionString);
            var dataSource = connection.DataSource; // Could be c:/something.db OR ./Something.db



            // Do we just create a file on disk in the connection string location?

            // Use DB in project directory.  If it does not exist, create it:
            //var connectionStringBuilder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
            //{
            //    DataSource = "./SqliteDB.db"
            //};



            // SQL CE example...
            //var engine = new System.Data.SqlServerCe.SqlCeEngine(ConnectionString);
            //engine.CreateDatabase();
        }
    }
}
