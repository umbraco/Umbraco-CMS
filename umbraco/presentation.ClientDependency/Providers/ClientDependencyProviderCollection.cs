using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration.Provider;

namespace umbraco.presentation.ClientDependency.Providers
{
	public class ClientDependencyProviderCollection : ProviderCollection
	{
		public new ClientDependencyProvider this[string name]
		{
			get { return (ClientDependencyProvider)base[name]; }
		}

		public override void Add(ProviderBase provider)
		{
			if (provider == null)
				throw new ArgumentNullException("provider");

			if (!(provider is ClientDependencyProvider))
				throw new ArgumentException("Invalid provider type", "provider");

			base.Add(provider);
		}
	}

}
