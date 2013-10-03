using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Umbraco.Core.Configuration.Dashboard
{
    internal class AccessElement : RawXmlConfigurationElement, IAccess
    {
        public AccessElement()
        {
            
        }

        public AccessElement(XElement rawXml)
            :base(rawXml)
        {
        }

        public IEnumerable<IAccessItem> Rules
        {
            get
            {
                var result = new List<AccessItem>();
                if (RawXml != null)
                {
                    result.AddRange(RawXml.Elements("deny").Select(x => new AccessItem {Action = AccessType.Deny, Value = x.Value }));
                    result.AddRange(RawXml.Elements("grant").Select(x => new AccessItem { Action = AccessType.Grant, Value = x.Value }));
                    result.AddRange(RawXml.Elements("grantBySection").Select(x => new AccessItem { Action = AccessType.GrantBySection, Value = x.Value }));
                }
                return result;
            }
        }
    }
}