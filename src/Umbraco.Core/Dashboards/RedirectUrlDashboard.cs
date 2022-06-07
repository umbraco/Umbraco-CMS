using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Dashboards;

[Weight(20)]
public class RedirectUrlDashboard : IDashboard
{
    public string Alias => "contentRedirectManager";

    public string[] Sections => new[] { "content" };

    public string View => "views/dashboard/content/redirecturls.html";

    public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
}
