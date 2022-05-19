using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Dashboards;

[Weight(10)]
public class SettingsDashboard : IDashboard
{
    public string Alias => "settingsWelcome";

    public string[] Sections => new[] { "settings" };

    public string View => "views/dashboard/settings/settingsdashboardintro.html";

    public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
}
