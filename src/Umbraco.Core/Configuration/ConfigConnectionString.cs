using System;
using System.Data.Common;

namespace Umbraco.Cms.Core.Configuration
{
    public class ConfigConnectionString
    {
        public ConfigConnectionString(string name, string connectionString, string providerName = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ConnectionString = connectionString;

            ProviderName = string.IsNullOrEmpty(providerName) ? ParseProvider(connectionString) : providerName;
        }

        public string ConnectionString { get; }
        public string ProviderName { get; }
        public string Name { get; }

        private static bool IsSqlCe(DbConnectionStringBuilder builder) => (builder.TryGetValue("Data Source", out var ds)
                                                                           || builder.TryGetValue("DataSource", out ds)) &&
                                                                          ds is string dataSource &&
                                                                          dataSource.EndsWith(".sdf");

        private static bool IsSqlServer(DbConnectionStringBuilder builder) =>
            !string.IsNullOrEmpty(GetServer(builder)) &&
            ((builder.TryGetValue("Database", out var db) && db is string database &&
              !string.IsNullOrEmpty(database)) ||
             (builder.TryGetValue("AttachDbFileName", out var a) && a is string attachDbFileName &&
              !string.IsNullOrEmpty(attachDbFileName)) ||
             (builder.TryGetValue("Initial Catalog", out var i) && i is string initialCatalog &&
              !string.IsNullOrEmpty(initialCatalog)));

        private static string GetServer(DbConnectionStringBuilder builder)
        {
            if(builder.TryGetValue("Server", out var s) && s is string server)
            {
                return server;
            }

            if ((builder.TryGetValue("Data Source", out var ds)
                 || builder.TryGetValue("DataSource", out ds)) && ds is string dataSource)
            {
                return dataSource;
            }

            return "";
        }

        private static string ParseProvider(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                return null;
            }

            var builder = new DbConnectionStringBuilder {ConnectionString = connectionString};
            if (IsSqlCe(builder))
            {
                return Constants.DbProviderNames.SqlCe;
            }


            if (IsSqlServer(builder))
            {
                return Constants.DbProviderNames.SqlServer;
            }

            throw new ArgumentException("Cannot determine provider name from connection string",
                nameof(connectionString));
        }
    }
}
