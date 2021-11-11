// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Data.Common;
using Microsoft.Extensions.Configuration;

namespace Umbraco.Extensions
{
    /// <summary>
    /// Extension methods for configuration.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Gets the provider name for the connection string name (shorthand for <c>GetSection("ConnectionStrings")[name + "_ProviderName"]</c>).
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="name">The connection string key.</param>
        /// <returns>The provider name.</returns>
        /// <remarks>
        /// This uses the same convention as the <a href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-5.0#connection-string-prefixes-1">Configuration API for connection string environment variables</a>.
        /// </remarks>
        public static string GetConnectionStringProviderName(this IConfiguration configuration, string name)
            => configuration.GetConnectionString(name + "_ProviderName");

        /// <summary>
        /// Gets the Umbraco connection string (shorthand for <c>GetSection("ConnectionStrings")[name]</c> and replacing the <c>|DataDirectory|</c> placeholder).
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="name">The connection string key.</param>
        /// <returns>The Umbraco connection string.</returns>
        public static string GetUmbracoConnectionString(this IConfiguration configuration, string name = Cms.Core.Constants.System.UmbracoConnectionName)
            => configuration.GetUmbracoConnectionString(name, out _);

        /// <summary>
        /// Gets the Umbraco connection string and provider name (shorthand for <c>GetSection("ConnectionStrings")[Constants.System.UmbracoConnectionName]</c> and replacing the <c>|DataDirectory|</c> placeholder).
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="providerName">The provider name.</param>
        /// <returns>The Umbraco connection string.</returns>
        public static string GetUmbracoConnectionString(this IConfiguration configuration, out string providerName)
            => configuration.GetUmbracoConnectionString(Cms.Core.Constants.System.UmbracoConnectionName, out providerName);

        /// <summary>
        /// Gets the Umbraco connection string and provider name (shorthand for <c>GetSection("ConnectionStrings")[name]</c> and replacing the <c>|DataDirectory|</c> placeholder).
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="name">The name.</param>
        /// <param name="providerName">The provider name.</param>
        /// <returns>The Umbraco connection string.</returns>
        public static string GetUmbracoConnectionString(this IConfiguration configuration, string name, out string providerName)
        {
            var connectionString = configuration.GetConnectionString(name);
            if (!string.IsNullOrEmpty(connectionString))
            {
                var builder = new DbConnectionStringBuilder
                {
                    ConnectionString = connectionString
                };

                // Replace data directory placeholder
                if (TryReplaceDataDirectory(builder))
                {
                    // Mutate the existing connection string (note: the builder also lowercases the properties)
                    connectionString = builder.ToString();
                }

                // Get or parse provider name
                providerName = configuration.GetConnectionStringProviderName(name);
                if (string.IsNullOrEmpty(providerName))
                {
                    providerName = ParseProviderName(builder);
                }
            }
            else
            {
                providerName = null;
            }

            return connectionString;
        }

        /// <summary>
        /// Gets the provider name or parse it from the Umbraco connection string.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="name">The connection string key.</param>
        /// <returns>The provider name.</returns>
        public static string GetUmbracoConnectionStringProviderName(this IConfiguration configuration, string name = Cms.Core.Constants.System.UmbracoConnectionName)
        {
            var providerName = configuration.GetConnectionStringProviderName(name);
            if (string.IsNullOrEmpty(providerName))
            {
                var connectionString = configuration.GetConnectionString(name);
                if (!string.IsNullOrEmpty(connectionString))
                {
                    var builder = new DbConnectionStringBuilder
                    {
                        ConnectionString = connectionString
                    };

                    providerName = ParseProviderName(builder);
                }
            }

            return providerName;
        }

        /// <summary>
        /// Replaces the <c>|DataDirectory|</c> placeholder in the <c>AttachDbFileName</c> key of the connection string.
        /// </summary>
        /// <param name="builder">The database connection string builder.</param>
        /// <returns><c>true</c> if the placeholder was replaced; otherwise, <c>false</c>.</returns>
        internal static bool TryReplaceDataDirectory(DbConnectionStringBuilder builder)
        {
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

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Parses the provider name from the connection string.
        /// </summary>
        /// <param name="builder">The database connection string builder.</param>
        /// <returns>The provider name.</returns>
        internal static string ParseProviderName(DbConnectionStringBuilder builder)
        {
            if ((builder.TryGetValue("Data Source", out var dataSourceValue) || builder.TryGetValue("DataSource", out dataSourceValue)) &&
                dataSourceValue is string dataSource &&
                dataSource.EndsWith(".sdf", StringComparison.OrdinalIgnoreCase))
            {
                return Cms.Core.Constants.DbProviderNames.SqlCe;
            }

            return Cms.Core.Constants.DbProviderNames.SqlServer;
        }
    }
}
