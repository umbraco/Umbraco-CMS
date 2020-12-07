// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Core.Configuration.Models
{
    /// <summary>
    /// Typed configuration options for connection strings.
    /// </summary>
    public class ConnectionStrings
    {
        // Backing field for UmbracoConnectionString to load from configuration value with key umbracoDbDSN.
        // Attributes cannot be applied to map from keys that don't match, and have chosen to retain the key name
        // used in configuration for older Umbraco versions.
        // See: https://stackoverflow.com/a/54607296/489433
#pragma warning disable SA1300  // Element should begin with upper-case letter
#pragma warning disable IDE1006 // Naming Styles
        private string umbracoDbDSN
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore SA1300  // Element should begin with upper-case letter
        {
            get => UmbracoConnectionString?.ConnectionString;
            set => UmbracoConnectionString = new ConfigConnectionString(Constants.System.UmbracoConnectionName, value);
        }

        /// <summary>
        /// Gets or sets a value for the Umbraco database connection string..
        /// </summary>
        public ConfigConnectionString UmbracoConnectionString { get; set; }
    }
}
