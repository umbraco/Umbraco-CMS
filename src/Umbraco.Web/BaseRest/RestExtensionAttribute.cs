using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Web.BaseRest
{
    [Obsolete("Umbraco /base is obsoleted, use WebApi (UmbracoApiController) instead for all REST based logic")]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class RestExtensionAttribute : Attribute
	{
		public string Alias { get; private set; }

		public RestExtensionAttribute(string alias)
		{
			this.Alias = alias;
		}
	}
}
