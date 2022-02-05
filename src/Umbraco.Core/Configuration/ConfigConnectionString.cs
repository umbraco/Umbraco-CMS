using System;
using System.Data.Common;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Configuration
{
    [Obsolete("Use the named options UmbracoConnectionString model instead.")]
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
            if (ConfigurationExtensions.TryReplaceDataDirectory(builder))
            {
                // Mutate the existing connection string (note: the builder also lowercases the properties)
                connectionString = builder.ToString();
            }

            // Also parse provider name now we already have a builder
            if (string.IsNullOrEmpty(providerName))
            {
                providerName = ConfigurationExtensions.ParseProviderName(builder);
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
            => UmbracoConnectionString.ParseProviderName(connectionString);
    }
}
