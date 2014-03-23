﻿using System.Configuration;

namespace Umbraco.Web.BaseRest.Configuration
{
	public class MethodElement : ConfigurationElement
	{
		const string KeyName = "name";
		const string KeyAllowAll = "allowAll";
		const string KeyAllowGroup = "allowGroup";
		const string KeyAllowType = "allowType";
		const string KeyAllowMember = "allowMember";
		const string KeyReturnXml = "returnXml";
		const string KeyContentType = "contentType";

		[ConfigurationProperty(KeyName, IsKey = true, IsRequired = true)]
		public string Name
		{
			get { return (string)base[KeyName]; }
		}

		[ConfigurationProperty(KeyAllowAll, IsKey = false, IsRequired = false, DefaultValue = false)]
		public bool AllowAll
		{
			get { return (bool)base[KeyAllowAll]; }
		}

		[ConfigurationProperty(KeyAllowGroup, IsKey = false, IsRequired = false, DefaultValue = null)]
		public string AllowGroup
		{
			get { return (string)base[KeyAllowGroup]; }
		}

		[ConfigurationProperty(KeyAllowType, IsKey = false, IsRequired = false, DefaultValue = null)]
		public string AllowType
		{
			get { return (string)base[KeyAllowType]; }
		}

		[ConfigurationProperty(KeyAllowMember, IsKey = false, IsRequired = false, DefaultValue = null)]
		public string AllowMember
		{
			get { return (string)base[KeyAllowMember]; }
		}

		[ConfigurationProperty(KeyReturnXml, IsKey = false, IsRequired = false, DefaultValue = true)]
		public bool ReturnXml
		{
			get { return (bool)base[KeyReturnXml]; }
		}

		[ConfigurationProperty(KeyContentType, IsKey = false, IsRequired = false, DefaultValue = null)]
		public string ContentType
		{
			get { return (string)base[KeyContentType]; }
		}
	}
}
