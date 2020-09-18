using System.Collections.Generic;

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
            get => UmbracoConnectionString?.ConnectionString;
            set => UmbracoConnectionString = new ConfigConnectionString(Constants.System.UmbracoConnectionName, value);
        }

        public ConfigConnectionString UmbracoConnectionString { get; set; }
    }
}
