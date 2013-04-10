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
	public sealed class ConfigurationKeyAttribute : Attribute
	{
	    /// <summary>
	    /// Initializes a new instance of the <see cref="ConfigurationKeyAttribute"/> class with a configuration key.
	    /// </summary>
	    /// <param name="configurationKey">The configurationkey.</param>
	    public ConfigurationKeyAttribute(string configurationKey)
	    {
	        ConfigurationKey = configurationKey;
	    }

        /// <summary>
        /// Gets or sets the configuration key.
        /// </summary>
        public string ConfigurationKey { get; private set; }
	}
}
