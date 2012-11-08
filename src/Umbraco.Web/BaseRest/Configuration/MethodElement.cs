using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Umbraco.Web.BaseRest.Configuration
{
	public class MethodElement : ConfigurationElement
	{
		const string Key_Name = "name";
		const string Key_AllowAll = "allowAll";
		const string Key_AllowGroup = "allowGroup";
		const string Key_AllowType = "allowType";
		const string Key_AllowMember = "allowMember";
		const string Key_ReturnXml = "returnXml";

		[ConfigurationProperty(Key_Name, IsKey = true, IsRequired = true)]
		public string Name
		{
			get { return (string)base[Key_Name]; }
		}

		[ConfigurationProperty(Key_AllowAll, IsKey = false, IsRequired = false, DefaultValue = false)]
		public bool AllowAll
		{
			get { return (bool)base[Key_AllowAll]; }
		}

		[ConfigurationProperty(Key_AllowGroup, IsKey = false, IsRequired = false, DefaultValue = null)]
		public string AllowGroup
		{
			get { return (string)base[Key_AllowGroup]; }
		}

		[ConfigurationProperty(Key_AllowType, IsKey = false, IsRequired = false, DefaultValue = null)]
		public string AllowType
		{
			get { return (string)base[Key_AllowType]; }
		}

		[ConfigurationProperty(Key_AllowMember, IsKey = false, IsRequired = false, DefaultValue = null)]
		public string AllowMember
		{
			get { return (string)base[Key_AllowMember]; }
		}

		[ConfigurationProperty(Key_ReturnXml, IsKey = false, IsRequired = false, DefaultValue = true)]
		public bool ReturnXml
		{
			get { return (bool)base[Key_ReturnXml]; }
		}
	}
}
