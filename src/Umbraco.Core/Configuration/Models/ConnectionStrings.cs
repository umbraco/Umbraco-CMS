using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Umbraco.Core.Configuration.Models
{
    public class ConnectionStrings
    {
        // Backing field for UmbracoConnectionString to load from configuration value with key umbracoDbDSN.
        // Attributes cannot be applied to map from keys that don't match, and have chosen to retain the key name
        // used in configuration for older Umbraco versions.
        // See: https://stackoverflow.com/a/54607296/489433
        private string umbracoDbDSN
        {
            get => string.Empty;
            set => UmbracoConnectionString = value;
        }

        public string UmbracoConnectionString { get; set; }

        private Dictionary<string, string> AsDictionary() => new Dictionary<string, string>
            {
                { Constants.System.UmbracoConnectionName, UmbracoConnectionString }
            };

        public ConfigConnectionString this[string key]
        {
            get
            {
                var connectionString = this.AsDictionary()[key];
                var provider = ParseProvider(connectionString);
                return new ConfigConnectionString(connectionString, provider, key);
            }
            set => throw new NotImplementedException();
        }

        private string ParseProvider(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                return null;
            }

            var builder = new DbConnectionStringBuilder();

            builder.ConnectionString = connectionString;

            if (builder.TryGetValue("Data Source", out var ds) && ds is string dataSource)
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
