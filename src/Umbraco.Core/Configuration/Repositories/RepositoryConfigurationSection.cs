using System;
using System.Configuration;

namespace Umbraco.Core.Configuration.Repositories
{
    internal class RepositoryConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("repositories", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(RepositoryCollection),
            AddItemName = "add",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public RepositoryCollection Repositories
        {
            get
            {
                return (RepositoryCollection)base["repositories"];
            }
        }
    }

    internal class RepositoryCollection : ConfigurationElementCollection
    {
        public RepositoryCollection()
        {
            Console.WriteLine("RepositoryCollection Constructor");
        }

        public RepositoryElement this[int index]
        {
            get { return (RepositoryElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public void Add(RepositoryElement repositoryElement)
        {
            BaseAdd(repositoryElement);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new RepositoryElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RepositoryElement)element).Name;
        }

        public void Remove(RepositoryElement repositoryElement)
        {
            BaseRemove(repositoryElement.Name);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(string name)
        {
            BaseRemove(name);
        }
    }

    internal class RepositoryElement : ConfigurationElement
    {
        private const string NameKey = "name";
        private const string ModelTypeKey = "modelType";
        private const string RepositoryTypeKey = "repositoryType";

        public RepositoryElement() { }

        public RepositoryElement(string name, string modelType, string repositoryType)
        {
            Name = name;
            ModelType = modelType;
            RepositoryType = repositoryType;
        }

        [ConfigurationProperty(NameKey, IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string)this[NameKey]; }

            set { this[NameKey] = value; }
        }

        [ConfigurationProperty(ModelTypeKey, IsRequired = true, IsKey = false)]
        public string ModelType
        {
            get { return (string)this[ModelTypeKey]; }

            set { this[ModelTypeKey] = value; }
        }

        [ConfigurationProperty(RepositoryTypeKey, IsRequired = true, IsKey = false)]
        public string RepositoryType
        {
            get { return (string)this[RepositoryTypeKey]; }

            set { this[RepositoryTypeKey] = value; }
        }
    }
}