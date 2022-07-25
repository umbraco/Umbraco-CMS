using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Dashboards;

[Weight(60)]
public class ProfilerDashboard : IDashboard
{
    public string Alias => "settingsProfiler";

    public string[] Sections => new[] { "settings" };

    public string View => "views/dashboard/settings/profiler.html";

    public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
}
