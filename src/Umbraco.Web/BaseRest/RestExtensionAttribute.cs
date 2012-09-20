using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Web.BaseRest
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class RestExtensionAttribute : Attribute
	{
		public string Alias { get; private set; }

		public RestExtensionAttribute(string alias)
		{
			this.Alias = alias;
		}
	}
}
