namespace Umbraco.Cms.Core.Dashboards;

public class AnalyticsDashboard : IDashboard
{
    public string Alias => "settingsAnalytics";

    public string[] Sections => new[] { "settings" };

    public string View => "views/dashboard/settings/analytics.html";

    public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
}
