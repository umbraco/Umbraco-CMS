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
            ConnectionString = ParseConnectionString(connectionString, ref providerName);
            ProviderName = providerName;
        }

        private static string ParseConnectionString(string connectionString, ref string providerName)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                return connectionString;
            }

            var builder = new DbConnectionStringBuilder
            {
                ConnectionString = connectionString
            };

            // Replace data directory placeholder
            const string attachDbFileNameKey = "AttachDbFileName";
            const string dataDirectoryPlaceholder = "|DataDirectory|";
            if (builder.TryGetValue(attachDbFileNameKey, out var attachDbFileNameValue) &&
                attachDbFileNameValue is string attachDbFileName &&
                attachDbFileName.Contains(dataDirectoryPlaceholder))
            {
                var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString();
                if (!string.IsNullOrEmpty(dataDirectory))
                {
                    builder[attachDbFileNameKey] = attachDbFileName.Replace(dataDirectoryPlaceholder, dataDirectory);

                    // Mutate the existing connection string (note: the builder also lowercases the properties)
                    connectionString = builder.ToString();
                }
            }

            // Also parse provider name now we already have a builder
            if (string.IsNullOrEmpty(providerName))
            {
                providerName = ParseProviderName(builder);
            }

            return connectionString;
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

            return ParseProviderName(builder);
        }

        private static string ParseProviderName(DbConnectionStringBuilder builder)
        {
            if ((builder.TryGetValue("Data Source", out var dataSource) || builder.TryGetValue("DataSource", out dataSource)) &&
                dataSource?.ToString().EndsWith(".sdf", StringComparison.OrdinalIgnoreCase) == true)
            {
                return Constants.DbProviderNames.SqlCe;
            }

            return Constants.DbProviderNames.SqlServer;
        }
    }
}
