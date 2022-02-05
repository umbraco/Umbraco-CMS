using System.Data.Common;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Configuration.Models
{
    /// <summary>
    /// Represents connection string details used by Umbraco.
    /// </summary>
    public class UmbracoConnectionString
    {
        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>
        /// The connection string.
        /// </value>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the provider name.
        /// </summary>
        /// <value>
        /// The provider name.
        /// </value>
        public string ProviderName { get; set; }

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

            return ConfigurationExtensions.ParseProviderName(builder);
        }
    }
}
