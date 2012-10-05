using System.Configuration;

namespace Umbraco.Core.Configuration.Repositories
{
    internal class RepositorySettings : ConfigurationSection
    {
        [ConfigurationProperty(RepositoryMappingConstants.ConfigurationPropertyName,
           IsDefaultCollection = true)]
        public RepositoryMappingCollection RepositoryMappings
        {
            get { return (RepositoryMappingCollection)base[RepositoryMappingConstants.ConfigurationPropertyName]; }
        }
    }
}