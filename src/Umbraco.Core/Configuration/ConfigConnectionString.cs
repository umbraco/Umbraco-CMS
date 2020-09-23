using System;
using System.Data.Common;

namespace Umbraco.Core.Configuration
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

        private string ParseProvider(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                return null;
            }

            var builder = new DbConnectionStringBuilder
            {
                ConnectionString = connectionString
            };

            if (
                (builder.TryGetValue("Data Source", out var ds)
                 || builder.TryGetValue("DataSource", out ds)) && ds is string dataSource)
            {
                if (dataSource.EndsWith(".sdf"))
                {
                    return Constants.DbProviderNames.SqlCe;
                }
            }


            if (builder.TryGetValue("Server", out var s) && s is string server && !string.IsNullOrEmpty(server))
            {
                if (builder.TryGetValue("Database", out var db) && db is string database && !string.IsNullOrEmpty(database))
                {
                    return Constants.DbProviderNames.SqlServer;
                }

                if (builder.TryGetValue("AttachDbFileName", out var a) && a is string attachDbFileName && !string.IsNullOrEmpty(attachDbFileName))
                {
                    return Constants.DbProviderNames.SqlServer;
                }

                if (builder.TryGetValue("Initial Catalog", out var i) && i is string initialCatalog && !string.IsNullOrEmpty(initialCatalog))
                {
                    return Constants.DbProviderNames.SqlServer;
                }
            }

            throw new ArgumentException("Cannot determine provider name from connection string", nameof(connectionString));
        }
    }
}
