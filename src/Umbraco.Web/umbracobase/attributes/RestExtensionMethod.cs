using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace umbraco.presentation.umbracobase
{
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
    public class RestExtensionMethod : System.Attribute
    {
        public bool allowAll;
        public string allowGroup;
        public string allowType;
        public string allowMember;
        public bool returnXml;

        public RestExtensionMethod()
        {
            returnXml = true;
            allowAll = true;
        }

        public bool GetAllowAll()
        {
            return allowAll;
        }

        public string GetAllowGroup()
        {
            return allowGroup;
        }

        public string GetAllowType()
        {
            return allowType;
        }

        public string GetAllowMember()
        {
            return allowMember;
        }
    }
}