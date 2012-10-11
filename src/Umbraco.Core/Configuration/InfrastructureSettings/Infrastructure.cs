using System.Configuration;

namespace Umbraco.Core.Configuration.InfrastructureSettings
{
    public class Infrastructure : ConfigurationSection
    {
        private const string InfrastructureSectionName = "umbraco/infrastructure";

        public static Infrastructure Instance
        {
            get { return (Infrastructure) ConfigurationManager.GetSection(InfrastructureSectionName); }
        }

        #region RepositoriesSection Property

        internal const string RepositoriesPropertyName = "repositories";

        [ConfigurationProperty(RepositoriesPropertyName, IsRequired = true, IsKey = false, IsDefaultCollection = false)]
        public Repositories Repositories
        {
            get { return ((Repositories)base[RepositoriesPropertyName]); }
            set { base[RepositoriesPropertyName] = value; }
        }

        #endregion

        #region PublishingStrategy Property

        internal const string PublishingStrategyPropertyName = "publishingStrategy";

        [ConfigurationProperty(PublishingStrategyPropertyName, IsRequired = true, IsKey = false, IsDefaultCollection = false)]
        public PublishingProvider PublishingStrategy
        {
            get { return ((PublishingProvider)base[PublishingStrategyPropertyName]); }
            set { base[PublishingStrategyPropertyName] = value; }
        }

        #endregion
    }

    public class Repositories : ConfigurationElement
    {
        [ConfigurationProperty("", IsDefaultCollection = true, IsRequired = true)]
        public RepositoryElementCollection Repository
        {
            get { return ((RepositoryElementCollection)(base[""])); }
        }
    }

    [ConfigurationCollection(typeof(Repository), CollectionType = ConfigurationElementCollectionType.BasicMapAlternate, AddItemName = RepositoryPropertyName)]
    public class RepositoryElementCollection : ConfigurationElementCollection
    {
        internal const string RepositoryPropertyName = "repository";

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMapAlternate;
            }
        }

        protected override string ElementName
        {
            get
            {
                return RepositoryPropertyName;
            }
        }

        protected override bool IsElementName(string elementName)
        {
            return elementName == RepositoryPropertyName;
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Repository)element).InterfaceShortTypeName;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new Repository();
        }

        #region Indexer

        public Repository this[int index]
        {
            get { return (Repository)base.BaseGet(index); }
        }

        public Repository this[string interfaceShortTypeName]
        {
            get { return (Repository)base.BaseGet(interfaceShortTypeName); }
        }

        #endregion

        #region Add

        public void Add(Repository repository)
        {
            BaseAdd(repository);
        }

        #endregion

        #region Remove

        public void Remove(Repository repository)
        {
            BaseRemove(repository);
        }

        #endregion

        #region GetItem

        public Repository GetItemAt(int index)
        {
            return (Repository)BaseGet(index);
        }

        public Repository GetItemByKey(string interfaceShortTypeName)
        {
            return (Repository)BaseGet(interfaceShortTypeName);
        }

        #endregion

        public bool ContainsKey(string interfaceShortName)
        {
            bool result = false;
            object[] keys = this.BaseGetAllKeys();
            foreach (object key in keys)
            {
                if ((string)key == interfaceShortName)
                {
                    result = true;
                    break;

                }
            }
            return result;
        }
    }

    public class Repository : ConfigurationElement
    {
        internal const string InterfaceShortTypeNamePropertyName = "interfaceShortTypeName";

        [ConfigurationPropertyAttribute(InterfaceShortTypeNamePropertyName, IsRequired = true, IsKey = true, IsDefaultCollection = false)]
        public string InterfaceShortTypeName
        {
            get { return (string) base[InterfaceShortTypeNamePropertyName]; }
            set { base[InterfaceShortTypeNamePropertyName] = value; }
        }

        internal const string RepositoryFullTypeNamePropertyName = "repositoryFullTypeName";

        [ConfigurationPropertyAttribute(RepositoryFullTypeNamePropertyName, IsRequired = true, IsKey = false, IsDefaultCollection = false)]
        public string RepositoryFullTypeName
        {
            get { return (string)base[RepositoryFullTypeNamePropertyName]; }
            set { base[RepositoryFullTypeNamePropertyName] = value; }
        }

        internal const string CacheProviderFullTypeNamePropertyName = "cacheProviderFullTypeName";

        [ConfigurationPropertyAttribute(CacheProviderFullTypeNamePropertyName, IsRequired = true, IsKey = false, IsDefaultCollection = false)]
        public string CacheProviderFullTypeName
        {
            get { return (string)base[CacheProviderFullTypeNamePropertyName]; }
            set { base[CacheProviderFullTypeNamePropertyName] = value; }
        }
    }

    public class PublishingProvider : ConfigurationElement
    {
        internal const string TypePropertyName = "type";

        [ConfigurationPropertyAttribute(TypePropertyName, IsRequired = true, IsKey = false, IsDefaultCollection = false)]
        public string Type
        {
            get { return (string)base[TypePropertyName]; }
            set { base[TypePropertyName] = value; }
        }
    }
}