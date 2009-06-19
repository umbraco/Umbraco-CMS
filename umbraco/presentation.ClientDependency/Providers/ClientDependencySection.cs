using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace umbraco.presentation.ClientDependency.Providers
{
	public class ClientDependencySection : ConfigurationSection
	{
		[ConfigurationProperty("providers")]
		public ProviderSettingsCollection Providers
		{
			get { return (ProviderSettingsCollection)base["providers"]; }
		}

		[StringValidator(MinLength = 1)]
		[ConfigurationProperty("defaultProvider", DefaultValue = "PageHeaderProvider")]
		public string DefaultProvider
		{
			get { return (string)base["defaultProvider"]; }
			set { base["defaultProvider"] = value; }
		}
	}

}
