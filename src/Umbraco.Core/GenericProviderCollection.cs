using System;
using System.Configuration.Provider;

namespace Umbraco.Core
{
    public class GenericProviderCollection<TProvider> : ProviderCollection where TProvider : ProviderBase
    {
        public override void Add(ProviderBase provider)
        {
            // make sure the provider supplied is not null
            if (provider == null)
                throw new ArgumentNullException("provider");

            if (provider as TProvider == null)
            {
                string providerTypeName = typeof(TProvider).ToString();
                throw new ArgumentException("Provider must implement provider type", providerTypeName);
            }

            base.Add(provider);
        }

        new public TProvider this[string name]
        {
            get
            {
                return (TProvider)base[name];
            }
        }
    }
}