using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Dashboards;

[Weight(10)]
public class MembersDashboard : IDashboard
{
    public string Alias => "memberIntro";

    public string[] Sections => new[] { "member" };

    public string View => "views/dashboard/members/membersdashboardvideos.html";

    public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
}
