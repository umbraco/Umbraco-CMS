using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Core.Configuration
{
	/// <summary>
	/// Indicates the configuration key for a section or a group.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	internal sealed class ConfigurationKeyAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ConfigurationKeyAttribute"/> class with a configuration key.
		/// </summary>
		/// <param name="configurationKey">The configurationkey.</param>
		/// <remarks>The default configuration key type is <c>Umbraco</c>.</remarks>
		public ConfigurationKeyAttribute(string configurationKey)
            : this(configurationKey, ConfigurationKeyType.Umbraco)
		{ }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationKeyAttribute"/> class with a configuration key and a key type.
        /// </summary>
        /// <param name="configurationKey">The configurationkey.</param>
        /// <param name="keyType">The key type.</param>
        public ConfigurationKeyAttribute(string configurationKey, ConfigurationKeyType keyType)
        {
            ConfigurationKey = configurationKey;
            KeyType = keyType;
        }

        /// <summary>
        /// Gets or sets the configuration key.
        /// </summary>
        public string ConfigurationKey { get; private set; }

        /// <summary>
        /// Gets or sets the configuration key type.
        /// </summary>
        public ConfigurationKeyType KeyType { get; private set; }
	}
}
