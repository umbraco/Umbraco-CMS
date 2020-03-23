using System;
using System.Data.Common;
using Microsoft.Extensions.Configuration;
using Umbraco.Core;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration.Models
{
    public class ConnectionStrings : IConnectionStrings
    {
        private readonly IConfiguration _configuration;

        public ConnectionStrings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ConfigConnectionString this[string key]
        {
            get
            {
                var connectionString = _configuration.GetConnectionString(key);
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

            if (builder.TryGetValue("Server", out var s) && s is string server && builder.TryGetValue("Database", out var db) && db is string database)
            {
                if (!string.IsNullOrEmpty(server) && !string.IsNullOrEmpty(database))
                {
                    return Constants.DbProviderNames.SqlServer;
                }
            }

            throw new ArgumentException("Cannot determine provider name from connection string", nameof(connectionString));
        }
    }
}
