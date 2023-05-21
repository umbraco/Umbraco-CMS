using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Dashboards;

[Weight(10)]
public class ContentDashboard : IDashboard
{
    public string Alias => "contentIntro";

    public string[] Sections => new[] { Constants.Applications.Content };

    public string View => "views/dashboard/default/startupdashboardintro.html";

    public IAccessRule[] AccessRules { get; } = Array.Empty<IAccessRule>();
}
