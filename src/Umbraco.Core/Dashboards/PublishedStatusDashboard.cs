using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Dashboards;

[Weight(30)]
public class PublishedStatusDashboard : IDashboard
{
    public string Alias => "settingsPublishedStatus";

    public string[] Sections => new[] { "settings" };

    public string View => "views/dashboard/settings/publishedstatus.html";

    public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
}
