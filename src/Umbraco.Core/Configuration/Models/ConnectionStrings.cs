// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;

namespace Umbraco.Cms.Core.Configuration.Models
{
    /// <summary>
    /// Typed configuration options for connection strings.
    /// </summary>
    [Obsolete("Use the named options UmbracoConnectionString model instead.")]
    [UmbracoOptions("ConnectionStrings", BindNonPublicProperties = true)]
    public class ConnectionStrings : UmbracoConnectionString
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
            get => ConnectionString;
            set
            {
                ConnectionString = value;
                ProviderName = ParseProviderName(value);
                UmbracoConnectionString = new ConfigConnectionString(Constants.System.UmbracoConnectionName, ConnectionString, ProviderName);
            }
        }

        /// <summary>
        /// Gets or sets a value for the Umbraco database connection string..
        /// </summary>
        public ConfigConnectionString UmbracoConnectionString { get; set; } = new ConfigConnectionString(Constants.System.UmbracoConnectionName, null);
    }
}
