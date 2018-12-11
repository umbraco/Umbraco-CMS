using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Umbraco.Core.Configuration.Dashboard
{
    internal class AccessElement : RawXmlConfigurationElement, IAccess
    {
        public AccessElement()
        { }

        public AccessElement(XElement rawXml)
            : base(rawXml)
        { }

        public IEnumerable<IAccessRule> Rules
        {
            get
            {
                var result = new List<AccessRule>();
                if (RawXml == null) return result;

                result.AddRange(RawXml.Elements("deny").Select(x => new AccessRule {Type = AccessRuleType.Deny, Value = x.Value }));
                result.AddRange(RawXml.Elements("grant").Select(x => new AccessRule { Type = AccessRuleType.Grant, Value = x.Value }));
                result.AddRange(RawXml.Elements("grantBySection").Select(x => new AccessRule { Type = AccessRuleType.GrantBySection, Value = x.Value }));
                return result;
            }
        }
    }
}
