using System.Configuration;

namespace Umbraco.Core.Configuration.Repositories
{
    internal sealed class RepositoryMappingElement : ConfigurationElement
    {
        [ConfigurationProperty(RepositoryMappingConstants.InterfaceShortTypeNameAttributeName,
            IsKey = true, IsRequired = true)]
        public string InterfaceShortTypeName
        {
            get
            {
                return (string)this[RepositoryMappingConstants.InterfaceShortTypeNameAttributeName];
            }
            set
            {
                this[RepositoryMappingConstants.InterfaceShortTypeNameAttributeName] = value;
            }
        }

        [ConfigurationProperty(RepositoryMappingConstants.RepositoryFullTypeNameAttributeName,
            IsRequired = true)]
        public string RepositoryFullTypeName
        {
            get
            {
                return (string)this[RepositoryMappingConstants.RepositoryFullTypeNameAttributeName];
            }
            set
            {
                this[RepositoryMappingConstants.RepositoryFullTypeNameAttributeName] = value;
            }
        }

        [ConfigurationProperty(RepositoryMappingConstants.CacheProviderFullTypeNameAttributeName,
            IsRequired = true)]
        public string CacheProviderFullTypeName
        {
            get
            {
                return (string)this[RepositoryMappingConstants.CacheProviderFullTypeNameAttributeName];
            }
            set
            {
                this[RepositoryMappingConstants.CacheProviderFullTypeNameAttributeName] = value;
            }
        }
    }
}