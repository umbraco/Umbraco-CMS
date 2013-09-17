using System;
using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.BaseRest
{
    public interface IExtensionsCollection : IEnumerable<IExtension>
    {
        IExtension this[string index] { get; }
    }

	[ConfigurationCollection(typeof(ExtensionElement), CollectionType = ConfigurationElementCollectionType.BasicMapAlternate)]
    internal class ExtensionElementCollection : ConfigurationElementCollection, IExtensionsCollection
	{
		const string KeyExtension = "extension";

		public override ConfigurationElementCollectionType CollectionType
		{
			get { return ConfigurationElementCollectionType.BasicMapAlternate; }
		}

		protected override string ElementName
		{
			get { return KeyExtension; }
		}

		protected override bool IsElementName(string elementName)
		{
			return elementName.Equals(KeyExtension, StringComparison.InvariantCultureIgnoreCase);
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new ExtensionElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((ExtensionElement)element).Alias;
		}

		public override bool IsReadOnly()
		{
			return false;
		}

		new public ExtensionElement this[string index]
		{
			get { return (ExtensionElement)BaseGet(index); }
		}

        IEnumerator<IExtension> IEnumerable<IExtension>.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return BaseGet(i) as IExtension;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IExtension IExtensionsCollection.this[string index]
        {
            get { return this[index]; }
        }
    }
}
