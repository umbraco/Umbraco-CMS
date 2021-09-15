using System;
using System.Data.Common;

namespace Umbraco.Cms.Core.Configuration
{
    public class ConfigConnectionString
    {
        public string Name { get; }

        public string ConnectionString { get; }

        public string ProviderName { get; }

        public ConfigConnectionString(string name, string connectionString, string providerName = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ConnectionString = connectionString;
            ProviderName = string.IsNullOrEmpty(providerName) ? ParseProviderName(connectionString) : providerName;
        }

        /// <summary>
        /// Parses the connection string to get the provider name.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>
        /// The provider name or <c>null</c> is the connection string is empty.
        /// </returns>
        public static string ParseProviderName(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                return null;
            }

            var builder = new DbConnectionStringBuilder
            {
                ConnectionString = connectionString
            };

            if ((builder.TryGetValue("Data Source", out var dataSource) || builder.TryGetValue("DataSource", out dataSource)) &&
                dataSource?.ToString().EndsWith(".sdf", StringComparison.OrdinalIgnoreCase) == true)
            {
                return Cms.Core.Constants.DbProviderNames.SqlCe;
            }

            return Cms.Core.Constants.DbProviderNames.SqlServer;
        }
    }
}
