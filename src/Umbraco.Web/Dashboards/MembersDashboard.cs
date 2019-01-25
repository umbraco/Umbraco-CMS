using System;
using Umbraco.Core.Composing;
using Umbraco.Core.Dashboards;

namespace Umbraco.Web.Dashboards
{
    [Weight(10)]
    public class MembersDashboard : IDashboardSection
    {
        public string Alias => "memberIntro";

        public string[] Sections => new [] { "member" };

        public string View => "views/dashboard/members/membersdashboardvideos.html";

        public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
    }
}
