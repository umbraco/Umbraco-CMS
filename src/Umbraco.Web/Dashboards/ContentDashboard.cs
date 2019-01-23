using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Core.Composing;
using Umbraco.Core.Dashboards;

namespace Umbraco.Web.Dashboards
{
    [Weight(10)]
    //[HideFromTypeFinder]
    [DataContract]
    public class ContentDashboard : IDashboardSection
    {
        [DataMember(Name = "name")]
        public string Name => "Get Started";

        [DataMember(Name = "alias")]
        public string Alias => "contentIntro";

        [IgnoreDataMember]
        public string[] Sections => new string[] { "content" };

        [DataMember(Name = "view")]
        public string View => "views/dashboard/default/startupdashboardintro.html";

        [IgnoreDataMember]
        public IAccessRule[] AccessRules
        {
            get
            {
                //TODO: WB Not convinced these rules work correctly?!
                var rules = new List<IAccessRule>();
                rules.Add(new AccessRule { Type = AccessRuleType.Deny, Value = "translator" });
                rules.Add(new AccessRule { Type = AccessRuleType.Grant, Value = "admin" });
                return rules.ToArray();
            }
        }
    }
}
