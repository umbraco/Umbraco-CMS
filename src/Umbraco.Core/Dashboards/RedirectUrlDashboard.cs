using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Dashboards;

[Weight(20)]
public class RedirectUrlDashboard : IDashboard
{
    public string Alias => "contentRedirectManager";

    public string[] Sections => new[] { Constants.Applications.Content };

    public string View => "views/dashboard/content/redirecturls.html";

    public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
}
