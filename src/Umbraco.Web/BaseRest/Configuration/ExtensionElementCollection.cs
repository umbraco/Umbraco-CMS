using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Umbraco.Web.BaseRest.Configuration
{
	[ConfigurationCollection(typeof(ExtensionElement), CollectionType = ConfigurationElementCollectionType.BasicMapAlternate)]
	public class ExtensionElementCollection : ConfigurationElementCollection
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
	}
}
