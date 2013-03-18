using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Umbraco.Web.BaseRest.Configuration
{
	[ConfigurationCollection(typeof(ExtensionElement), CollectionType = ConfigurationElementCollectionType.BasicMapAlternate)]
	public class ExtensionElement : ConfigurationElementCollection
	{
		const string KeyAlias = "alias";
		const string KeyType = "type";
		const string KeyMethod = "method";

		[ConfigurationProperty(KeyAlias, IsKey = true, IsRequired = true)]
		public string Alias
		{
			get { return (string)base[KeyAlias]; }
		}

		[ConfigurationProperty(KeyType, IsKey = false, IsRequired = true)]
		public string Type
		{
			get { return (string)base[KeyType]; }
		}

		public override ConfigurationElementCollectionType CollectionType
		{
			get { return ConfigurationElementCollectionType.BasicMapAlternate; }
		}

		protected override string ElementName
		{
			get { return KeyMethod; }
		}

		protected override bool IsElementName(string elementName)
		{
			return elementName.Equals(KeyMethod, StringComparison.InvariantCultureIgnoreCase);
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new MethodElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((MethodElement)element).Name;
		}

		public override bool IsReadOnly()
		{
			return false;
		}

		new public MethodElement this[string index]
		{
			get { return (MethodElement)BaseGet(index); }
		}
	}
}
