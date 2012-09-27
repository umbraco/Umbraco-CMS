using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Umbraco.Web.BaseRest.Configuration
{
	public class BaseRestSection : ConfigurationSection
	{
		[ConfigurationProperty("", IsKey = false, IsRequired = false, IsDefaultCollection = true)]
		public ExtensionElementCollection Items
		{
			get { return (ExtensionElementCollection)base[""]; }
		}
	}
}
