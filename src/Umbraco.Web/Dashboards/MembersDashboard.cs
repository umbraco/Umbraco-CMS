using System;
using System.Runtime.Serialization;
using Umbraco.Core.Composing;
using Umbraco.Core.Dashboards;

namespace Umbraco.Web.Dashboards
{
    [Weight(10)]
    [DataContract]
    public class MembersDashboard : IDashboardSection
    {
        [DataMember(Name = "name")]
        public string Name => "Get Started";

        [DataMember(Name = "alias")]
        public string Alias => "memberIntro";

        [IgnoreDataMember]
        public string[] Sections => new string[] { "member" };

        [DataMember(Name = "view")]
        public string View => "views/dashboard/members/membersdashboardvideos.html";

        [IgnoreDataMember]
        public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
    }
}
