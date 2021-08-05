// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Configuration.Models
{
    public class NamedConnectionString
    {
        /// <summary>
        /// Gets or sets the named connection string's alias.
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the named connection string value.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets the connection string as a <see cref="Configuration.ConfigConnectionString"/>.
        /// </summary>
        public ConfigConnectionString ConfigConnectionString => new ConfigConnectionString(Alias, ConnectionString);
    }
}
