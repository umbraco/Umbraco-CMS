using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Web.BaseRest
{
    [Obsolete("Umbraco /base is obsoleted, use WebApi (UmbracoApiController) instead for all REST based logic")]
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public sealed class RestExtensionMethodAttribute : Attribute
	{
		public bool AllowAll { get; set; }
		public string AllowGroup { get; set; }
		public string AllowType { get; set; }
		public string AllowMember { get; set; }
		public bool ReturnXml { get; set; }

		public RestExtensionMethodAttribute()
		{
			this.AllowAll = true;
			this.ReturnXml = true;
		}
	}
}
