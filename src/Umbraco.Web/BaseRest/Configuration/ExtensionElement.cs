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
		const string Key_Alias = "alias";
		const string Key_Type = "type";
		const string Key_Method = "method";

		[ConfigurationProperty(Key_Alias, IsKey = true, IsRequired = true)]
		public string Alias
		{
			get { return (string)base[Key_Alias]; }
		}

		[ConfigurationProperty(Key_Type, IsKey = false, IsRequired = true)]
		public string Type
		{
			get { return (string)base[Key_Type]; }
		}

		public override ConfigurationElementCollectionType CollectionType
		{
			get { return ConfigurationElementCollectionType.BasicMapAlternate; }
		}

		protected override string ElementName
		{
			get { return Key_Method; }
		}

		protected override bool IsElementName(string elementName)
		{
			return elementName.Equals(Key_Method, StringComparison.InvariantCultureIgnoreCase);
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
