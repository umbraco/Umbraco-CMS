using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace umbraco.presentation.umbracobase
{
	[Obsolete("Use Umbraco.Web.BaseRest.RestExtensionAttribute")]
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false)]
    public class RestExtension : System.Attribute
    {
        string alias;

        public RestExtension(string alias)
        {

            this.alias = alias;
        }

        public string GetAlias()
        {
            return alias;
        }
    }
}