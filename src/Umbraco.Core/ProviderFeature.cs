using System;
using System.Configuration;
using System.Configuration.Provider;
using System.Web.Configuration;
using Umbraco.Core.Configuration;

namespace Umbraco.Core
{
    public abstract class ProviderFeature<TProvider> where TProvider : ProviderBase
    {
        private static bool _initialized;
        private static object _lock = new object();

        public static TProvider Provider { get; private set; }

        public static GenericProviderCollection<TProvider> Providers { get; private set; }

        protected static void Initialize(string sectionName)
        {
            if (!_initialized)
            {
                lock (_lock)
                {
                    if (_initialized)
                        return;

                    var section = (ProviderFeatureSection) ConfigurationManager.GetSection(sectionName);
                    if (section == null)
                        throw new Exception(string.Format("{0} is not configured for this application", sectionName));

                    Providers = new GenericProviderCollection<TProvider>();

                    ProvidersHelper.InstantiateProviders(section.Providers, Providers, typeof (TProvider));

                    Provider = Providers[section.DefaultProvider];

                    _initialized = true;
                }
            }
        }
    }
}