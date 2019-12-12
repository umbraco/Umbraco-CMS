using System.Data.Common;
using Umbraco.Core.Persistence;

namespace Umbraco.Web
{
    public class UmbracoDbProviderFactoryCreator : IDbProviderFactoryCreator
    {
        private readonly string _defaultProviderName;

        public UmbracoDbProviderFactoryCreator(string defaultProviderName)
        {
            _defaultProviderName = defaultProviderName;
        }

        public DbProviderFactory CreateFactory()
        {
            return CreateFactory(_defaultProviderName);
        }

        public DbProviderFactory CreateFactory(string providerName)
        {
            if (string.IsNullOrEmpty(providerName)) return null;

            return DbProviderFactories.GetFactory(providerName);
        }
    }
}
